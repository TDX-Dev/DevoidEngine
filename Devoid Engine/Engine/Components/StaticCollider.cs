using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class StaticCollider : Component
    {
        public override string Type => nameof(StaticCollider);

        public PhysicsShapeDescription Shape = new PhysicsShapeDescription
        {
            Type = PhysicsShapeType.Box,
            Size = new Vector3(1, 1, 1)
        };

        public PhysicsMaterial Material = PhysicsMaterial.Default;

        private GameObject _debugObject;

        public bool DebugDraw = false;

        public override void OnStart()
        {
            // Create physics static
            var desc = new PhysicsStaticDescription
            {
                Position = gameObject.transform.Position,
                Rotation = gameObject.transform.Rotation,
                Shape = Shape,
                Material = Material
            };

            gameObject.Scene.Physics.CreateStatic(desc, gameObject);

            if (DebugDraw)
                CreateDebugVisual();
        }

        private void CreateDebugVisual()
        {
            if (Shape.Type != PhysicsShapeType.Box)
                return;

            // Create child object
            _debugObject = gameObject.Scene.addGameObject("StaticCollider_Debug");

            _debugObject.transform.Position = gameObject.transform.Position;
            _debugObject.transform.Rotation = gameObject.transform.Rotation;
            _debugObject.transform.Scale = Shape.Size;

            // Create cube mesh
            Mesh cubeMesh = new Mesh();
            cubeMesh.SetVertices(Primitives.GetCubeVertex());

            // Add MeshRenderer
            var meshRenderer = _debugObject.AddComponent<MeshRenderer>();
            meshRenderer.AddMesh(cubeMesh);

            // Optional: assign debug material if needed
            // meshRenderer.SetMaterial(DebugMaterial);
        }

        public override void OnUpdate(float dt)
        {
            if (_debugObject == null)
                return;

            // Keep debug object synced with collider transform
            _debugObject.transform.Position = gameObject.transform.Position;
            _debugObject.transform.Rotation = gameObject.transform.Rotation;
        }

        public override void OnDestroy()
        {
            if (_debugObject != null)
            {
                gameObject.Scene.Destroy(_debugObject);
                _debugObject = null;
            }
        }
    }
}