using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class PortalCubeComponent : Component
    {
        public override string Type => nameof(PortalCubeComponent);

        public RigidBodyComponent Body { get; private set; }

        public bool IsHeld { get; private set; }

        private FPSController holder;

        public float HoldDistance = 3f;

        public override void OnStart()
        {
            Body = gameObject.GetComponent<RigidBodyComponent>();
        }

        public void PickUp(FPSController player)
        {
            if (Body == null) return;

            holder = player;
            IsHeld = true;

            Body.SetKinematic(false); // ensure dynamic
            Body.LinearVelocity = Vector3.Zero;
            Body.AngularVelocity = Vector3.Zero;
            Body.WakeUp();

        }

        public void Drop()
        {
            if (Body == null)
                return;

            IsHeld = false;

            Body.SetKinematic(false);

            Vector3 releaseVelocity = Vector3.Zero;

            if (holder != null)
            {
                // Only inherit player body velocity
                releaseVelocity = holder.GetLinearVelocity();
            }

            Body.LinearVelocity = releaseVelocity;
            Body.AngularVelocity = Vector3.Zero;
            Body.WakeUp();

            holder = null;
        }

        public override void OnUpdate(float dt)
        {
            if (!IsHeld || holder == null)
                return;

            var pivot = holder.GetCameraPivot();
            if (pivot == null)
                return;

            Vector3 forward =
                Vector3.Normalize(
                    Vector3.Transform(Vector3.UnitZ, pivot.Rotation)
                );

            Vector3 targetPos = pivot.Position + forward * HoldDistance;

            Vector3 positionError = targetPos - Body.Position;

            // DIRECT VELOCITY TARGET (not force-based)
            float followSpeed = 20f;  // increase for tighter feel

            Vector3 targetVelocity = positionError * followSpeed;

            // Smooth toward that velocity
            float responsiveness = 15f;

            Body.LinearVelocity = Vector3.Lerp(
                Body.LinearVelocity,
                targetVelocity,
                responsiveness * dt
            );

            Body.AngularVelocity = Vector3.Zero;
        }
    }
}