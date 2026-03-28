using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidStandaloneLauncher.Scripts
{
    public class FreeCameraComponent : Component
    {
        public override string Type => nameof(FreeCameraComponent);

        // ===== SETTINGS =====
        public float MoveSpeed = 8f;
        public float BoostMultiplier = 3f;
        public float MouseSensitivity = 0.12f;
        public float MinPitch = -89f;
        public float MaxPitch = 89f;

        // ===== INTERNAL =====
        private float yaw;
        private float pitch;

        public override void OnStart()
        {
            // Initialize yaw/pitch from current forward
            Vector3 forward = gameObject.Transform.Forward;

            yaw = MathHelper.RadToDeg(
                MathF.Atan2(forward.X, forward.Z)
            );

            pitch = MathHelper.RadToDeg(
                MathF.Asin(forward.Y)
            );
        }

        public override void OnUpdate(float dt)
        {
            HandleMouseLook();
            HandleMovement(dt);
        }

        // =========================================
        // Mouse Look
        // =========================================
        private void HandleMouseLook()
        {
            float mouseDeltaX = Input.GetAction("LookX");
            float mouseDeltaY = Input.GetAction("LookY");

            // DO NOT multiply mouse delta by dt
            yaw -= mouseDeltaX * MouseSensitivity;
            pitch += mouseDeltaY * MouseSensitivity;

            pitch = Math.Clamp(pitch, MinPitch, MaxPitch);

            gameObject.Transform.Rotation =
                Quaternion.CreateFromYawPitchRoll(
                    MathHelper.DegToRad(yaw),
                    MathHelper.DegToRad(pitch),
                    0f);
        }

        // =========================================
        // Movement
        // =========================================
        private void HandleMovement(float dt)
        {
            Quaternion rotation = gameObject.Transform.Rotation;

            Vector3 forward =
                Vector3.Normalize(
                    Vector3.Transform(Vector3.UnitZ, rotation));

            Vector3 right =
                Vector3.Normalize(
                    Vector3.Transform(-Vector3.UnitX, rotation));

            Vector3 up =
                Vector3.Normalize(
                    Vector3.Transform(Vector3.UnitY, rotation));

            Vector3 move = Vector3.Zero;

            if (Input.GetAction("Forward") == 1) move += forward;
            if (Input.GetAction("Backward") == 1) move -= forward;
            if (Input.GetAction("Right") == 1) move += right;
            if (Input.GetAction("Left") == 1) move -= right;
            if (Input.GetAction("Up") == 1) move += up;
            if (Input.GetAction("Down") == 1) move -= up;

            if (Input.GetActionDown("Capture"))
            {
                if (Cursor.GetCursorState() == CursorState.Grabbed)
                {
                    Cursor.SetCursorState(CursorState.Normal);
                } else
                {
                    Cursor.SetCursorState(CursorState.Grabbed);
                }
                Console.WriteLine(Cursor.GetCursorState());
            }

            if (move.LengthSquared() > 0)
                move = Vector3.Normalize(move);

            float speed = MoveSpeed;
            if (Input.GetKey(Keys.LeftShift))
                speed *= BoostMultiplier;

            gameObject.Transform.Position += move * speed * dt;
        }
    }
}