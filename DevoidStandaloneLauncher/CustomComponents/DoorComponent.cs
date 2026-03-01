using DevoidEngine.Engine.Core;
using System;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class DoorComponent : Component
    {
        public override string Type => nameof(DoorComponent);

        public float OpenAngle = 90f;
        public float TurnSpeed = 4f;

        private bool isOpen = false;
        private bool isTurning = false;

        private Quaternion closedRotation;
        private Quaternion openRotation;

        private Quaternion startRotation;
        private Quaternion targetRotation;

        private float turnProgress = 0f;

        public override void OnStart()
        {
            // Store closed state as initial rotation
            closedRotation = gameObject.transform.Rotation;

            Quaternion delta =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitY,
                    MathF.PI / 180f * OpenAngle
                );

            openRotation = closedRotation * delta;
        }

        // -----------------------------------------------------
        // New deterministic Turn method
        // -----------------------------------------------------
        public void Turn(bool open)
        {
            if (isTurning)
                return;

            // If already in requested state → do nothing
            if (open == isOpen)
                return;

            isTurning = true;
            turnProgress = 0f;

            startRotation = gameObject.transform.Rotation;
            targetRotation = open ? openRotation : closedRotation;

            isOpen = open;
        }

        public override void OnUpdate(float dt)
        {
            if (!isTurning)
                return;

            turnProgress += dt * TurnSpeed;

            float t = Math.Clamp(turnProgress, 0f, 1f);

            gameObject.transform.Rotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            if (t >= 1f)
                isTurning = false;
        }

        // Optional helper
        public bool IsOpen => isOpen;
    }
}