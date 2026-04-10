using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class StaticColliderComponent : Component
    {
        public override string Type => nameof(StaticColliderComponent);

        internal PhysicsShapeDescription internalShape = new PhysicsShapeDescription
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
                CreateStatic();
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

        //private void CreateDebugVisual()
        //{
        //    if (Shape.Type != PhysicsShapeType.Box)
        //        return;

        //    // Create child object
        //    _debugObject = gameObject.Scene.addGameObject("StaticCollider_Debug");

        //    _debugObject.transform.Position = gameObject.transform.Position;
        //    _debugObject.transform.Rotation = gameObject.transform.Rotation;
        //    _debugObject.transform.Scale = Shape.Size;

        //    // Create cube mesh
        //    Mesh cubeMesh = new Mesh();
        //    cubeMesh.SetVertices(Primitives.GetCubeVertex());

        //    // Add MeshRenderer
        //    var meshRenderer = _debugObject.AddComponent<MeshRenderer>();
        //    meshRenderer.AddMesh(cubeMesh);

        //    // Optional: assign debug material if needed
        //    // meshRenderer.SetMaterial(DebugMaterial);
        //}

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
            Matrix4x4 model = Matrix4x4.CreateFromQuaternion(gameObject.Transform.Rotation) * Matrix4x4.CreateScale(Shape.Size) * Matrix4x4.CreateTranslation(gameObject.Transform.Position);

            DebugRenderSystem.DrawCube(model);
        }

        public override void OnDestroy()
        {
            if (internalStatic != null)
                gameObject.Scene.Physics.RemoveStatic(internalStatic);
        }
    }
}