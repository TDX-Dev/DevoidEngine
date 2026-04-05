using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class RigidBodyComponent : Component
    {
        public override string Type => nameof(RigidBodyComponent);

        // ===============================
        // Settings
        // ===============================

        public float Mass
        {
            get => _mass;
            set
            {
                _mass = value;

                if (internalBody != null)
                    CreateBody();
            }
        }

        public bool StartKinematic = false;

        public bool OverrideRotation = false;

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

        private IPhysicsBody? internalBody;

        public PhysicsShapeDescription internalShape = new PhysicsShapeDescription
        {
            Type = PhysicsShapeType.Box,
            Size = new Vector3(1, 1, 1)
        };

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
            get => internalBody != null ? internalBody.Position : gameObject.Transform.Position;
            set
            {
                if (internalBody != null)
                {
                    internalBody.Position = value;
                }
            }
        }

        public Quaternion Rotation
        {
            get => internalBody != null ? internalBody.Rotation : gameObject.Transform.Rotation;
            set
            {
                if (internalBody != null)
                    internalBody.Rotation = value;
            }
        }

        public bool AllowSleep
        {
            get => allowSleep;
            set
            {
                allowSleep = value;
                CreateBody();
            }
        }

        private float _mass;
        public bool IsKinematic =>
            internalBody != null && internalBody.IsKinematic;

        public bool allowSleep = true;

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
                StartKinematic = true;
                internalBody.IsKinematic = true;
            }
            else
            {
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
                Position = gameObject.Transform.Position,
                Rotation = gameObject.Transform.Rotation,
                Mass = _mass,
                IsKinematic = StartKinematic,
                Shape = Shape,
                Material = Material,
                AllowSleep = allowSleep
            };

            internalBody = gameObject.Scene.Physics.CreateBody(desc, gameObject);
        }

        public override void OnUpdate(float dt)
        {
            Matrix4x4 model = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Shape.Size) * Matrix4x4.CreateTranslation(gameObject.Transform.Position);

            DebugRenderSystem.DrawCube(model);

            if (internalBody == null)
                return;

            if (internalBody.IsKinematic)
            {
                internalBody.Position = gameObject.Transform.Position;
                internalBody.Rotation = gameObject.Transform.Rotation;
            }
        }

        public override void OnFixedUpdate(float dt)
        {
            if (internalBody == null)
                return;

            //if (!internalBody.IsKinematic)
            //{
            //    // Freeze rotations by zeroing angular velocity
            //    Vector3 angVel = internalBody.AngularVelocity;

            //    if (FreezeRotationX) angVel.X = 0f;
            //    if (FreezeRotationY) angVel.Y = 0f;
            //    if (FreezeRotationZ) angVel.Z = 0f;

            //    internalBody.AngularVelocity = angVel;

            //    //gameObject.Transform.Position = internalBody.Position;
            //    if (OverrideRotation)
            //    {
            //        internalBody.Rotation = gameObject.Transform.Rotation;
            //    }
            //    else
            //    {
            //        //gameObject.Transform.Rotation = internalBody.Rotation;
            //    }
            //}
        }

        public override void OnRender()
        {
        }

        public override void OnDestroy()
        {
            internalBody?.Remove();
            internalBody = null;
        }

        public void AddImpulse(Vector3 impulse)
        {
            internalBody?.AddImpulse(impulse);
        }

        public void AddForce(Vector3 force)
        {
            if (internalBody != null)
                internalBody.AddForce(force);
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

        public void WakeUp()
        {
            internalBody?.WakeUp();
        }
    }
}