using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Physics;
using System.Numerics;

namespace DevoidEngine.Nodes
{
    public class StaticbodyNode : Node3D
    {
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

        protected override void OnStart()
        {
            CreateStatic();
        }

        private void CreateStatic()
        {
            if (internalStatic != null)
            {
                Scene.Physics.RemoveStatic(internalStatic);
                internalStatic = null;
            }

            var desc = new PhysicsStaticDescription
            {
                Position = Transform.Position,
                Rotation = Transform.Rotation,
                Shape = internalShape,
                Material = internalMaterial
            };

            internalStatic = Scene.Physics.CreateStatic(desc, this);
        }
        protected override void OnDestroy()
        {
            if (internalStatic != null)
            {
                Scene.Physics.RemoveStatic(internalStatic);
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
