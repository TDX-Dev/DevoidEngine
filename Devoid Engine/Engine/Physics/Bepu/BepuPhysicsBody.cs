using BepuPhysics;
using DevoidEngine.Engine.Physics;
using System.Numerics;

namespace DevoidEngine.Engine.Physics.Bepu
{
    internal class BepuPhysicsBody : IPhysicsBody
    {
        internal BodyHandle Handle;
        private Simulation simulation;

        internal PhysicsMaterial Material;


        private BepuPhysicsBackend backend;

        private static int nextId = 1;
        public int Id { get; }

        public BepuPhysicsBody(
            BodyHandle handle,
            Simulation simulation,
            PhysicsMaterial material,
            BepuPhysicsBackend backend)
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


        public void AddImpulse(Vector3 impulse)
        {
            var body = GetBody();
            body.ApplyLinearImpulse(impulse);
            body.Awake = true;
        }

        public void AddForce(Vector3 force, float dt)
        {
            var body = GetBody();

            // If inverse mass is zero → kinematic body
            if (body.LocalInertia.InverseMass == 0f)
                return;

            Vector3 impulse = force * dt;

            body.ApplyLinearImpulse(impulse);
            body.Awake = true;
        }

        public void AddTorque(Vector3 torque)
        {
            var body = GetBody();
            body.ApplyAngularImpulse(torque);
            body.Awake = true;
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