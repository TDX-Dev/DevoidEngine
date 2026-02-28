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

        private Quaternion startRotation;
        private Quaternion targetRotation;
        private RigidBodyComponent rigidBody;
        private float turnProgress = 0f;

        public override void OnStart()
        {
            startRotation = gameObject.transform.Rotation;
            //rigidBody = gameObject.ge
        }

        public void Turn()
        {
            Console.WriteLine("Turn!");
            if (isTurning)
                return;

            isTurning = true;
            turnProgress = 0f;

            startRotation = gameObject.transform.Rotation;

            float angle = isOpen ? -OpenAngle : OpenAngle;

            Quaternion delta =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitY,
                    MathF.PI / 180f * angle
                );

            targetRotation = startRotation * delta;

            isOpen = !isOpen;
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
    }
}