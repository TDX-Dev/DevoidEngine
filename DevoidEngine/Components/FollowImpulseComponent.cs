using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class FollowImpulseComponent : Component
    {
        public override string Type => nameof(FollowImpulseComponent);

        /// <summary>
        /// Object to move towards.
        /// </summary>
        public GameObject? FollowTarget;

        /// <summary>
        /// Strength of the impulse applied every frame.
        /// </summary>
        public float ImpulseStrength = 4.0f;

        /// <summary>
        /// Stop applying impulses once we're this close.
        /// </summary>
        public float StopDistance = 5f;

        private RigidBodyComponent rb = null!;

        public override void OnStart()
        {
            rb = gameObject.GetComponent<RigidBodyComponent>()!;
        }

        public float MaxSpeed = 2f;
        public float Steering = 25f;

        public override void OnFixedUpdate(float dt)
        {
            if (FollowTarget == null)
                return;

            Vector3 toTarget = FollowTarget.Transform.Position - gameObject.Transform.Position;

            if (toTarget.LengthSquared() <= StopDistance * StopDistance)
                return;

            Vector3 desiredVelocity = Vector3.Normalize(toTarget) * MaxSpeed;

            Vector3 steering = desiredVelocity - rb.LinearVelocity;

            float maxChange = Steering * dt;

            if (steering.LengthSquared() > maxChange * maxChange)
                steering = Vector3.Normalize(steering) * maxChange;

            rb.LinearVelocity += steering;
        }
    }
}