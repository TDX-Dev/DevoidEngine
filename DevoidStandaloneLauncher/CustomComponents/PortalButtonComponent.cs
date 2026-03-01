using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Utilities;
using System;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class PortalButtonComponent : Component, ICollisionListener
    {
        public override string Type => nameof(PortalButtonComponent);

        public float PressDepth = 0.35f;
        public float PressSpeed = 4f;
        public float RequiredMass = 1f;

        public bool IsPressed { get; private set; }

        public event Action OnPressed;
        public event Action OnReleased;

        private int overlappingBodies = 0;
        private Vector3 originalPosition;
        private float currentOffset = 0f;



        public override void OnStart()
        {
            originalPosition = gameObject.transform.LocalPosition;
        }

        public override void OnUpdate(float dt)
        {
            float target = IsPressed ? -PressDepth : 0f;

            currentOffset = MathHelper.Lerp(currentOffset, target, dt * PressSpeed);

            //gameObject.transform.LocalPosition =
            //    originalPosition + new Vector3(0, currentOffset, 0);
        }

        public void OnCollisionEnter(GameObject other)
        {
            //Console.WriteLine("Button Enter!");
            var rb = other.GetComponent<RigidBodyComponent>();
            if (rb == null) return;

            if (rb.Mass >= RequiredMass)
            {
                overlappingBodies++;

                if (!IsPressed)
                {
                    IsPressed = true;
                    OnPressed?.Invoke();
                }
            }
        }

        public void OnCollisionExit(GameObject other)
        {
            //Console.WriteLine("Button Exit!");
            var rb = other.GetComponent<RigidBodyComponent>();
            if (rb == null) return;

            overlappingBodies--;

            if (overlappingBodies <= 0)
            {
                overlappingBodies = 0;

                if (IsPressed)
                {
                    IsPressed = false;
                    OnReleased?.Invoke();
                }
            }
        }

        public void OnCollisionStay(GameObject other)
        {

        }
    }
}