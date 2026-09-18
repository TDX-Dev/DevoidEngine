using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Physics;
using System.Numerics;

namespace DevoidEngine.Nodes
{
    public class RigidbodyNode : Node3D
    {
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
                if (internalBody != null)
                    CreateBody();
            }
        }

        public PhysicsMaterial Material = PhysicsMaterial.Default;

        public bool allowSleep = true;

        internal Vector3 SavedLinearVelocity;
        internal Vector3 SavedAngularVelocity;

        private IPhysicsBody? internalBody;

        internal PhysicsShapeDescription internalShape = new()
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
            get => internalBody != null ? internalBody.Position : Transform.Position;
            set
            {
                if (internalBody != null)
                    internalBody.Position = value;
            }
        }

        public Quaternion Rotation
        {
            get => internalBody != null ? internalBody.Rotation : Transform.Rotation;
            set
            {
                if (internalBody != null)
                    internalBody.Rotation = value;
            }
        }

        public bool IsKinematic =>
            internalBody != null && internalBody.IsKinematic;

        protected override void OnStart()
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
            if (Scene == null)
                return;

            if (internalBody != null)
            {
                Scene.Physics.RemoveBody(internalBody);
            }

            if (Shape.Type == PhysicsShapeType.Box && Shape.Size == Vector3.Zero)
            {
                internalShape.Size = new Vector3(1, 1, 1);
            }

            var desc = new PhysicsBodyDescription
            {
                Position = Transform.Position,
                Rotation = Transform.Rotation,
                Mass = Mass,
                IsKinematic = StartKinematic,
                Shape = Shape,
                Material = Material,
                AllowSleep = allowSleep,
                IsTrigger = false,
                AllowRotationX = !LockRotationX,
                AllowRotationY = !LockRotationY,
                AllowRotationZ = !LockRotationZ,
                CollisionDetectionSettings = PhysicsCollisionDetectionSettings.Continuous
            };

            internalBody = Scene.Physics.CreateBody(desc, this);
        }

        protected override void OnUpdate(float dt)
        {
            if (internalBody == null)
                return;
        }

        protected override void OnDestroy()
        {
            if (internalBody != null)
            {
                SavedLinearVelocity = internalBody.LinearVelocity;
                SavedAngularVelocity = internalBody.AngularVelocity;

                Scene.Physics.RemoveBody(internalBody);
                internalBody = null;
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

        public void OnDrawGizmos(GizmoContext context)
        {
            switch (Shape.Type)
            {
                case PhysicsShapeType.Box:
                    {
                        Vector3 halfSize = Shape.Size * 0.5f;
                        Vector3 position = Transform.Position;

                        context.DrawList.AddWireBox(position - halfSize, position + halfSize, GizmoCategory.Physics);

                        break;
                    }

                case PhysicsShapeType.Sphere:
                    {
                        // Draw sphere gizmo here.
                        break;
                    }

                case PhysicsShapeType.Capsule:
                    {
                        // Draw capsule gizmo here.
                        break;
                    }

                case PhysicsShapeType.Mesh:
                    {


                        // Draw mesh gizmo here.
                        break;
                    }
            }
        }
    }
}
