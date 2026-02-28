using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using System;
using System.Numerics;

namespace DevoidEngine.Engine.Components
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
            Vector3 forward = gameObject.transform.Forward;

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
            // DO NOT multiply mouse delta by dt
            yaw -= Input.MouseDelta.X * MouseSensitivity;
            pitch += Input.MouseDelta.Y * MouseSensitivity;

            pitch = Math.Clamp(pitch, MinPitch, MaxPitch);

            gameObject.transform.Rotation =
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
            Quaternion rotation = gameObject.transform.Rotation;

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

            if (Input.GetKey(Keys.W)) move += forward;
            if (Input.GetKey(Keys.S)) move -= forward;
            if (Input.GetKey(Keys.D)) move += right;
            if (Input.GetKey(Keys.A)) move -= right;
            if (Input.GetKey(Keys.E)) move += up;
            if (Input.GetKey(Keys.Q)) move -= up;

            if (move.LengthSquared() > 0)
                move = Vector3.Normalize(move);

            float speed = MoveSpeed;
            if (Input.GetKey(Keys.LeftShift))
                speed *= BoostMultiplier;

            gameObject.transform.Position += move * speed * dt;
        }
    }
}