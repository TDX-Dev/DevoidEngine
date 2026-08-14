using System;
using System.Numerics;
using DevoidEngine.Core;
using DevoidEngine.Rendering;
using ImGuiNET;

namespace Elemental
{
    public class EditorCamera
    {
        public Vector3 Position { get; set; } = new Vector3(0, 5, 10);
        public float Pitch { get; set; } = -20f; // Degrees
        public float Yaw { get; set; } = -90f;   // Degrees (pointing down -Z)

        public float Fov { get; set; } = 60f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000f;
        public float AspectRatio { get; set; } = 16f / 9f;

        public float MoveSpeed { get; set; } = 10f;
        public float MouseSensitivity { get; set; } = 0.15f;

        public Vector3 Forward { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }

        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }

        public Camera Camera { get; } = new Camera();

        public EditorCamera(float fov = 60f, float aspectRatio = 1.777f, float near = 0.1f, float far = 1000f)
        {
            Fov = fov;
            AspectRatio = aspectRatio;
            NearPlane = near;
            FarPlane = far;

            Recalculate();
        }

        public void OnUpdate(float deltaTime, bool isHovered)
        {
            // Only capture input when Right Mouse Button is held down
            if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
            {
                // 1. Mouse Look
                Vector2 mouseDelta = ImGui.GetIO().MouseDelta;
                Yaw += mouseDelta.X * MouseSensitivity;
                Pitch -= mouseDelta.Y * MouseSensitivity;
                Pitch = Math.Clamp(Pitch, -89f, 89f);

                // 2. Adjust Flight Speed via Mouse Wheel
                float wheel = ImGui.GetIO().MouseWheel;
                if (wheel != 0)
                {
                    MoveSpeed = MathF.Max(0.5f, MoveSpeed + wheel * 2.0f);
                }

                // 3. Movement
                float speed = MoveSpeed * deltaTime;
                if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                {
                    speed *= 2.5f; // Speed boost
                }

                if (ImGui.IsKeyDown(ImGuiKey.W)) Position += Forward * speed;
                if (ImGui.IsKeyDown(ImGuiKey.S)) Position -= Forward * speed;
                if (ImGui.IsKeyDown(ImGuiKey.A)) Position -= Right * speed;
                if (ImGui.IsKeyDown(ImGuiKey.D)) Position += Right * speed;
                if (ImGui.IsKeyDown(ImGuiKey.E)) Position += Vector3.UnitY * speed; // Ascend
                if (ImGui.IsKeyDown(ImGuiKey.Q)) Position -= Vector3.UnitY * speed; // Descend
            }

            Recalculate();
        }

        public void SetAspectRatio(float aspectRatio)
        {
            if (MathF.Abs(AspectRatio - aspectRatio) > 0.001f)
            {
                AspectRatio = aspectRatio;
                Recalculate();
            }
        }

        public void LookAt(Vector3 target)
        {
            Vector3 dir = Vector3.Normalize(target - Position);
            Pitch = MathF.Asin(dir.Y) * (180f / MathF.PI);
            Yaw = MathF.Atan2(dir.Z, dir.X) * (180f / MathF.PI);
            Recalculate();
        }

        private void Recalculate()
        {
            // Compute directional vectors from Pitch & Yaw
            float pitchRad = Pitch * (MathF.PI / 180f);
            float yawRad = Yaw * (MathF.PI / 180f);

            Vector3 dir;
            dir.X = MathF.Cos(yawRad) * MathF.Cos(pitchRad);
            dir.Y = MathF.Sin(pitchRad);
            dir.Z = MathF.Sin(yawRad) * MathF.Cos(pitchRad);

            Forward = Vector3.Normalize(dir);
            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));

            // Compute Matrices
            ViewMatrix = Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                Fov * (MathF.PI / 180f),
                AspectRatio,
                NearPlane,
                FarPlane
            );

            // Sync with DevoidEngine internal Camera state
            Camera.UpdateView(Position, Forward, Up);
            Camera.FOV = Fov;
            Camera.Near = NearPlane;
            Camera.Far = FarPlane;
            Camera.UpdateProjectionMatrix(AspectRatio);
        }
    }
}