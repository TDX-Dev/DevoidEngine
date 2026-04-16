using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.GizmoSystem;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Serialization;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class RigidBodyComponent : Component
    {
        public override string Type => nameof(RigidBodyComponent);

        public float Mass = 100f;

        public bool StartKinematic = false;

        public bool OverrideRotation = false;

        public bool LockRotationX = false;
        public bool LockRotationY = false;
        public bool LockRotationZ = false;

        public PhysicsShapeDescription Shape
        {
            get => internalShape;
            set
            {
                internalShape = value;

                // Recreate physics body if it already exists
                if (internalBody != null)
                    CreateBody();
            }
        }

        public PhysicsMaterial Material = PhysicsMaterial.Default;

        public bool allowSleep = true;

        internal Vector3 SavedLinearVelocity;
        internal Vector3 SavedAngularVelocity;


        [DontSerialize]
        private IPhysicsBody? internalBody;

        internal PhysicsShapeDescription internalShape = new PhysicsShapeDescription
        {
            Type = PhysicsShapeType.Box,
            Size = new Vector3(1, 1, 1)
        };


        public Vector3 LinearVelocity
        {
            get => internalBody != null ? internalBody.LinearVelocity : SavedLinearVelocity;
            set
            {
                SavedLinearVelocity = value;

                if (internalBody != null)
                {
                    internalBody.WakeUp();
                    internalBody.LinearVelocity = value;
                }
            }
        }

        public Vector3 AngularVelocity
        {
            get => internalBody != null ? internalBody.AngularVelocity : SavedAngularVelocity;
            set
            {
                SavedAngularVelocity = value;

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
                    internalBody.Position = value;
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

        public bool IsKinematic =>
            internalBody != null && internalBody.IsKinematic;

        public override void OnStart()
        {
            CreateBody();

            if (internalBody != null)
            {
                internalBody.LinearVelocity = SavedLinearVelocity;
                internalBody.AngularVelocity = SavedAngularVelocity;
            }
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
            if (gameObject.Scene == null)
                return;

            if (internalBody != null)
            {
                gameObject.Scene.Physics.RemoveBody(internalBody);
            }

            if (Shape.Type == PhysicsShapeType.Box && Shape.Size == Vector3.Zero)
            {
                internalShape.Size = new Vector3(1, 1, 1);
            }

            var desc = new PhysicsBodyDescription
            {
                Position = gameObject.Transform.Position,
                Rotation = gameObject.Transform.Rotation,
                Mass = Mass,
                IsKinematic = StartKinematic,
                Shape = Shape,
                Material = Material,
                AllowSleep = allowSleep,
                IsTrigger = false,
                AllowRotationX = !LockRotationX,
                AllowRotationY = !LockRotationY,
                AllowRotationZ = !LockRotationZ,
            };

            internalBody = gameObject.Scene.Physics.CreateBody(desc, gameObject);
        }

        public override void OnUpdate(float dt)
        {
            if (internalBody == null)
                return;

            Matrix4x4 model = Helper.BuildModel(
                internalBody.Position,
                Shape.Size,
                internalBody.Rotation);

            Gizmos.DrawCube(model, GizmoCategory.Physics);
        }

        public override void OnFixedUpdate(float dt)
        {
        }

        public override void OnRender()
        {
            Matrix4x4 model = Matrix4x4.CreateFromQuaternion(gameObject.Transform.Rotation) * Matrix4x4.CreateScale(Shape.Size) * Matrix4x4.CreateTranslation(gameObject.Transform.Position);

            Gizmos.DrawCube(model, GizmoCategory.Physics);
        }

        public override void OnDestroy()
        {
            if (internalBody != null)
            {
                SavedLinearVelocity = internalBody.LinearVelocity;
                SavedAngularVelocity = internalBody.AngularVelocity;

                gameObject.Scene.Physics.RemoveBody(internalBody);
            }
        }

        public void AddImpulse(Vector3 impulse)
        {
            internalBody?.AddImpulse(impulse);
        }

        public void AddForce(Vector3 force)
        {
            internalBody?.AddForce(force);
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