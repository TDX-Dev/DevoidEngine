using BepuPhysics;
using System.Numerics;

namespace DevoidEngine.Physics.Bepu
{
    internal class BepuPhysicsBody : IPhysicsBody
    {
        internal readonly BodyHandle Handle;
        private readonly Simulation simulation;

        internal PhysicsMaterial Material;
        private Vector3 accumulatedForce;
        private Vector3 accumulatedTorque;


        private readonly BepuPhysicsBackend backend;

        private static int nextId = 1;
        public int Id { get; }

        public BepuPhysicsBody(BodyHandle handle, Simulation simulation, PhysicsMaterial material, BepuPhysicsBackend backend)
        {
            Id = Interlocked.Increment(ref nextId);
            Handle = handle;
            this.simulation = simulation;
            Material = material;
            this.backend = backend;

        }



        private BodyReference GetBody()
        {
            return simulation.Bodies.GetBodyReference(Handle);
        }

        public Vector3 Position
        {
            get
            {
                var body = GetBody();
                return body.Pose.Position;
            }
            set
            {
                var body = GetBody();
                body.Pose.Position = value;
                body.UpdateBounds(); // important if teleporting
                body.Awake = true;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                var body = GetBody();
                return body.Pose.Orientation;
            }
            set
            {
                var body = GetBody();
                body.Pose.Orientation = value;
                body.UpdateBounds();
                body.Awake = true;
            }
        }

        public Vector3 LinearVelocity
        {
            get
            {
                var body = GetBody();
                return body.Velocity.Linear;
            }
            set
            {
                var body = GetBody();
                body.Velocity.Linear = value;
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                var body = GetBody();
                return body.Velocity.Angular;
            }
            set
            {
                var body = GetBody();
                body.Velocity.Angular = value;
            }
        }

        public float Mass
        {
            get
            {
                var body = GetBody();
                float invMass = body.LocalInertia.InverseMass;

                if (invMass == 0f)
                    return 0f;

                return 1f / invMass;
            }
        }

        public bool IsKinematic
        {
            get
            {
                var body = GetBody();
                return body.Kinematic;
            }
            set
            {
                var body = GetBody();

                if (value)
                    body.BecomeKinematic();
                else
                    throw new NotImplementedException("Switching to dynamic requires inertia setup.");
            }
        }

        internal void ApplyAccumulatedForces(float dt)
        {
            var body = GetBody();

            if (body.Kinematic)
            {
                accumulatedForce = Vector3.Zero;
                accumulatedTorque = Vector3.Zero;
                return;
            }

            if (accumulatedForce != Vector3.Zero)
                body.ApplyLinearImpulse(accumulatedForce * dt);

            if (accumulatedTorque != Vector3.Zero)
                body.ApplyAngularImpulse(accumulatedTorque * dt);

            accumulatedForce = Vector3.Zero;
            accumulatedTorque = Vector3.Zero;
        }

        public void AddImpulse(Vector3 impulse)
        {
            var body = GetBody();
            body.ApplyLinearImpulse(impulse);
            body.Awake = true;
        }

        public void AddForce(Vector3 force)
        {
            accumulatedForce += force;
            WakeUp();
        }

        public void AddTorque(Vector3 torque)
        {
            accumulatedTorque += torque;
            WakeUp();
        }

        public void AddImpulseAtPoint(Vector3 impulse, Vector3 worldOffset)
        {
            var body = GetBody();
            body.ApplyImpulse(impulse, worldOffset);
            body.Awake = true;
        }

        public void ApplyDamping(float dt)
        {
            var body = GetBody();
            body.Velocity.Linear *= (1f - Material.LinearDamping * dt);
            body.Velocity.Angular *= (1f - Material.AngularDamping * dt);
        }

        public void WakeUp()
        {
            var body = GetBody();
            body.Awake = true;
        }

        public void Sleep()
        {
            var body = GetBody();
            body.Awake = false;
        }

        public void Remove()
        {
            backend.RemoveBody(this);
        }
    }
}
