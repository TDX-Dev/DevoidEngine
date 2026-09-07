using DevoidEngine.Gizmos;
using DevoidEngine.Physics;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class StaticColliderComponent : Component, IGizmoProviderComponent
    {
        public override string Type => nameof(StaticColliderComponent);

        internal PhysicsShapeDescription internalShape = new()
        {
            Type = PhysicsShapeType.Box,
            Size = new Vector3(1, 1, 1)
        };

        public PhysicsShapeDescription Shape
        {
            get => internalShape;
            set
            {
                internalShape = value;
                if (IsInitialized)
                    CreateStatic();
                // Otherwise it'll initialize on start anyway
            }
        }

        internal PhysicsMaterial internalMaterial = PhysicsMaterial.Default;

        public PhysicsMaterial Material
        {
            get => internalMaterial;
            set
            {
                internalMaterial = value;
                CreateStatic();
            }
        }

        private IPhysicsStatic? internalStatic;

        public bool DebugDraw = false;

        public override void OnStart()
        {
            CreateStatic();
        }

        private void CreateStatic()
        {
            if (internalStatic != null)
            {
                gameObject.Scene.Physics.RemoveStatic(internalStatic);
                internalStatic = null;
            }

            var desc = new PhysicsStaticDescription
            {
                Position = gameObject.Transform.Position,
                Rotation = gameObject.Transform.Rotation,
                Shape = internalShape,
                Material = internalMaterial
            };

            internalStatic = gameObject.Scene.Physics.CreateStatic(desc, gameObject);
        }

        public override void OnRender()
        {
            //Matrix4x4 model = Matrix4x4.CreateFromQuaternion(gameObject.Transform.Rotation) * Matrix4x4.CreateScale(Shape.Size) * Matrix4x4.CreateTranslation(gameObject.Transform.Position);

            //Gizmos.DrawCube(model, GizmoCategory.Physics);
        }

        public override void OnDestroy()
        {
            if (internalStatic != null)
            {
                gameObject.Scene.Physics.RemoveStatic(internalStatic);
                internalStatic = null;
            }
        }

        public void OnDrawGizmos(GizmoContext context)
        {
            switch (Shape.Type)
            {
                case PhysicsShapeType.Box:
                    {
                        Vector3 halfSize = Shape.Size * 0.5f;
                        Vector3 position = gameObject.Transform.Position;

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
