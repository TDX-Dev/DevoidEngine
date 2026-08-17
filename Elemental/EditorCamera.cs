using System;
using System.Numerics;
using DevoidEngine.Core;
using DevoidEngine.Rendering;
using ImGuiNET;

namespace Elemental
{
    public class EditorCamera
    {
        public bool IsFocusing => isFocusing;
        public Vector3 Position { get; set; } = new Vector3(0, 5, 10);
        public Vector3 FocalPoint { get; set; } = Vector3.Zero;
        public float Distance { get; set; } = 10f;

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

        private bool isFocusing;
        private float focusTime;
        private const float FocusDuration = 0.5f;

        private Vector3 focusStartPosition;
        private Vector3 focusStartFocalPoint;

        private Vector3 focusTargetPosition;
        private Vector3 focusTargetFocalPoint;

        public EditorCamera(float fov = 60f, float aspectRatio = 1.777f, float near = 0.1f, float far = 1000f)
        {
            Fov = fov;
            AspectRatio = aspectRatio;
            NearPlane = near;
            FarPlane = far;

            // Compute initial distance and focal point based on position & orientation
            Recalculate();
            Distance = Vector3.Distance(Position, FocalPoint);
        }

        public void OnUpdate(float deltaTime, bool isHovered)
        {
            if (isFocusing)
            {
                UpdateFocus(deltaTime);

                // Don't allow normal camera movement to fight the focus animation.
                Recalculate();
                return;
            }

            Vector2 mouseDelta = ImGui.GetIO().MouseDelta;
            float wheel = ImGui.GetIO().MouseWheel;

            // -------------------------------------------------------------
            // 1. Right Mouse Button: Fly Cam Mode (FPS Style)
            // -------------------------------------------------------------
            if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
            {
                // Mouse Look
                Yaw += mouseDelta.X * MouseSensitivity;
                Pitch -= mouseDelta.Y * MouseSensitivity;
                Pitch = Math.Clamp(Pitch, -89f, 89f);

                // Flight Speed Adjustment via Scroll Wheel
                if (wheel != 0)
                {
                    MoveSpeed = MathF.Max(0.5f, MoveSpeed + wheel * 2.0f);
                }

                // Movement
                float speed = MoveSpeed * deltaTime;
                if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                {
                    speed *= 2.5f;
                }

                float forward = (ImGui.IsKeyDown(ImGuiKey.W) ? 1f : 0f) - (ImGui.IsKeyDown(ImGuiKey.S) ? 1f : 0f);
                float right = (ImGui.IsKeyDown(ImGuiKey.D) ? 1f : 0f) - (ImGui.IsKeyDown(ImGuiKey.A) ? 1f : 0f);
                float up = (ImGui.IsKeyDown(ImGuiKey.E) ? 1f : 0f) - (ImGui.IsKeyDown(ImGuiKey.Q) ? 1f : 0f);

                Vector3 moveDir = Vector3.Zero;
                moveDir += Forward * forward;
                moveDir -= Right * right;
                moveDir += Vector3.UnitY * up;

                if (moveDir != Vector3.Zero)
                {
                    moveDir = Vector3.Normalize(moveDir);
                }

                Position += moveDir * speed;

                // Sync FocalPoint so Orbit mode seamlessly picks up from current position
                FocalPoint = Position + Forward * Distance;
            }
            // -------------------------------------------------------------
            // 2. Middle Mouse Button: Blender Orbit / Pan Mode
            // -------------------------------------------------------------
            else if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
            {
                if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                {
                    // Pan (MMB + Shift) - moves both target & camera along local view plane
                    float panSpeed = Distance * 0.002f; // Scale speed with distance for smooth feel
                    Vector3 pan = (Right * mouseDelta.X + Up * mouseDelta.Y) * panSpeed;

                    FocalPoint += pan;
                    Position += pan;
                }
                else
                {
                    // Orbit around FocalPoint (MMB)
                    Yaw += mouseDelta.X * MouseSensitivity;
                    Pitch -= mouseDelta.Y * MouseSensitivity;
                    Pitch = Math.Clamp(Pitch, -89f, 89f);

                    RecalculateOrientation();
                    Position = FocalPoint - Forward * Distance;
                }
            }
            // -------------------------------------------------------------
            // 3. Zooming via Scroll Wheel (when RMB is not held)
            // -------------------------------------------------------------
            else if (wheel != 0 && isHovered)
            {
                // Exponential zoom relative to current focal distance
                float zoomDelta = wheel * (Distance * 0.1f);
                Distance = MathF.Max(0.1f, Distance - zoomDelta);
                Position = FocalPoint - Forward * Distance;
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
            FocalPoint = target;
            Vector3 dir = Vector3.Normalize(target - Position);
            Pitch = MathF.Asin(dir.Y) * (180f / MathF.PI);
            Yaw = MathF.Atan2(dir.Z, dir.X) * (180f / MathF.PI);
            Distance = Vector3.Distance(Position, FocalPoint);
            Recalculate();
        }

        public void FocusObject(GameObject obj)
        {
            if (obj == null)
                return;

            Vector3 target = obj.Transform.Position;

            Vector3 scale = obj.Transform.Scale;

            float radius = MathF.Max(
                MathF.Abs(scale.X),
                MathF.Max(
                    MathF.Abs(scale.Y),
                    MathF.Abs(scale.Z)));

            radius = MathF.Max(radius, 0.5f);

            float distance = radius * 3.0f;

            // Current camera state
            focusStartPosition = Position;
            focusStartFocalPoint = FocalPoint;

            // Desired camera state
            focusTargetFocalPoint = target;
            focusTargetPosition = target - Forward * distance;

            focusTime = 0f;
            isFocusing = true;
        }

        private void UpdateFocus(float deltaTime)
        {
            focusTime += deltaTime;

            float t = Math.Clamp(
                focusTime / FocusDuration,
                0f,
                1f);

            // Smoothstep
            t = t * t * (3f - 2f * t);

            Position = Vector3.Lerp(
                focusStartPosition,
                focusTargetPosition,
                t);

            FocalPoint = Vector3.Lerp(
                focusStartFocalPoint,
                focusTargetFocalPoint,
                t);

            if (focusTime >= FocusDuration)
            {
                Position = focusTargetPosition;
                FocalPoint = focusTargetFocalPoint;
                isFocusing = false;
            }
        }



        private void RecalculateOrientation()
        {
            float pitchRad = Pitch * (MathF.PI / 180f);
            float yawRad = -Yaw * (MathF.PI / 180f);

            Vector3 dir;
            dir.X = MathF.Cos(yawRad) * MathF.Cos(pitchRad);
            dir.Y = MathF.Sin(pitchRad);
            dir.Z = MathF.Sin(yawRad) * MathF.Cos(pitchRad);

            Forward = Vector3.Normalize(dir);
            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
        }

        private void Recalculate()
        {
            RecalculateOrientation();

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