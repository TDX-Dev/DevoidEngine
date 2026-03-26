using BepuPhysics;
using BepuUtilities;
using System.Numerics;

namespace DevoidEngine.Engine.Physics.Bepu
{

    internal struct BepuPoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;
        public float LinearDamping;
        public float AngularDamping;


        private Vector3Wide gravityWideDt;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public bool AllowSubstepsForUnconstrainedBodies => false;

        public bool IntegrateVelocityForKinematics => false;

        public void Initialize(Simulation simulation)
        {
            // Convert gravity into wide representation once
            gravityWideDt = Vector3Wide.Broadcast(Gravity);
        }

        public void PrepareForIntegration(float dt)
        {
            gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        public void IntegrateVelocity(
            Vector<int> bodyIndices,
            Vector3Wide position,
            QuaternionWide orientation,
            BodyInertiaWide localInertia,
            Vector<int> integrationMask,
            int workerIndex,
            Vector<float> dt,
            ref BodyVelocityWide velocity)
        {
            velocity.Linear += gravityWideDt;
        }
    }

}