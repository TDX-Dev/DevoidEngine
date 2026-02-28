using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class RigidBodyComponent : Component
    {
        public override string Type => nameof(RigidBodyComponent);

        // ===============================
        // Settings
        // ===============================

        public float Mass = 10f;
        public bool StartKinematic = false;

        public bool FreezeRotationX = false;
        public bool FreezeRotationY = false;
        public bool FreezeRotationZ = false;

        public PhysicsShapeDescription Shape
        {
            get => internalShape;
            set
            {
                internalShape = value;
                CreateBody();
            }
        }

        public PhysicsMaterial Material = PhysicsMaterial.Default;

        // ===============================
        // Internal Physics Handle
        // ===============================

        private IPhysicsBody internalBody;

        private PhysicsShapeDescription internalShape = new PhysicsShapeDescription
        {
            Type = PhysicsShapeType.Box,
            Size = new Vector3(1,1,1)
        };

        private PhysicsMaterial internalMaterial;

        // ===============================
        // Public Physics API
        // ===============================

        public Vector3 LinearVelocity
        {
            get => internalBody != null ? internalBody.LinearVelocity : Vector3.Zero;
            set
            {
                if (internalBody != null)
                {
                    internalBody.WakeUp();
                    internalBody.LinearVelocity = value;
                }
            }
        }

        public Vector3 AngularVelocity
        {
            get => internalBody != null ? internalBody.AngularVelocity : Vector3.Zero;
            set
            {
                if (internalBody != null)
                {
                    internalBody.WakeUp();
                    internalBody.AngularVelocity = value;
                }
            }
        }

        public Vector3 Position
        {
            get => internalBody != null ? internalBody.Position : gameObject.transform.Position;
            set
            {
                if (internalBody != null)
                    internalBody.Position = value;
            }
        }

        public Quaternion Rotation
        {
            get => internalBody != null ? internalBody.Rotation : gameObject.transform.Rotation;
            set
            {
                if (internalBody != null)
                    internalBody.Rotation = value;
            }
        }

        public bool IsKinematic =>
            internalBody != null && internalBody.IsKinematic;

        // ===============================
        // Lifecycle
        // ===============================

        public override void OnStart()
        {
            CreateBody();
        }

        public void SetKinematic(bool value)
        {
            if (internalBody == null)
                return;

            if (value == internalBody.IsKinematic)
                return;

            if (value)
            {
                internalBody.IsKinematic = true;
            }
            else
            {
                // Recreate as dynamic (clean way for now)
                StartKinematic = false;
                CreateBody();
            }
        }

        private void CreateBody()
        {
            if (internalBody != null)
            {
                gameObject.Scene.Physics.RemoveBody(internalBody);
            }

            if (internalShape.Type == PhysicsShapeType.Box && Shape.Size == Vector3.Zero)
            {
                internalShape.Size = new Vector3(1, 1, 1);
            }

            var desc = new PhysicsBodyDescription
            {
                Position = gameObject.transform.Position,
                Rotation = gameObject.transform.Rotation,
                Mass = Mass,
                IsKinematic = StartKinematic,
                Shape = Shape,
                Material = Material
            };

            internalBody = gameObject.Scene.Physics.CreateBody(desc, gameObject);
        }

        public override void OnUpdate(float dt)
        {
            if (internalBody == null)
                return;

            if (internalBody.IsKinematic)
            {
                internalBody.Position = gameObject.transform.Position;
                internalBody.Rotation = gameObject.transform.Rotation;
            }
        }

        public override void OnLateUpdate(float dt)
        {
            if (internalBody == null)
                return;

            if (!internalBody.IsKinematic)
            {
                // Freeze rotations by zeroing angular velocity
                Vector3 angVel = internalBody.AngularVelocity;

                if (FreezeRotationX) angVel.X = 0f;
                if (FreezeRotationY) angVel.Y = 0f;
                if (FreezeRotationZ) angVel.Z = 0f;

                internalBody.AngularVelocity = angVel;

                gameObject.transform.Position = internalBody.Position;
                gameObject.transform.Rotation = internalBody.Rotation;
            }
        }

        public override void OnDestroy()
        {
            internalBody?.Remove();
            internalBody = null;
        }

        // ===============================
        // Force API
        // ===============================

        public void AddImpulse(Vector3 impulse)
        {
            internalBody?.AddImpulse(impulse);
        }

        public void AddForce(Vector3 force)
        {
            if (internalBody != null)
                internalBody.AddForce(force, gameObject.Scene.fixedDeltaTime);
        }

        public void AddTorque(Vector3 torque)
        {
            internalBody?.AddTorque(torque);
        }

        public void SetLinearVelocity(Vector3 velocity)
        {
            LinearVelocity = velocity;
        }

        public void SetAngularVelocity(Vector3 velocity)
        {
            AngularVelocity = velocity;
        }
    }
}
