using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class AreaComponent : Component, ICollisionListener
    {
        public override string Type => nameof(AreaComponent);


        // ===============================
        // Settings
        // ===============================

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

        public bool AllowSleep
        {
            get => allowSleep;
            set
            {
                allowSleep = value;
                CreateBody();
            }
        }

        // ===============================
        // Internal Physics Handle
        // ===============================

        private IPhysicsBody? internalBody;

        private PhysicsShapeDescription internalShape = new PhysicsShapeDescription
        {
            Type = PhysicsShapeType.Box,
            Size = new Vector3(1, 1, 1)
        };

        private bool allowSleep = false;

        // ===============================
        // Events
        // ===============================

        public event Action<GameObject>? OnEnter;
        public event Action<GameObject>? OnExit;

        // ===============================
        // Lifecycle
        // ===============================

        public override void OnStart()
        {
            CreateBody();
        }

        private void CreateBody()
        {
            if (internalBody != null)
            {
                gameObject.Scene.Physics.RemoveBody(internalBody);
            }

            if (internalShape.Type == PhysicsShapeType.Box && internalShape.Size == Vector3.Zero)
            {
                internalShape.Size = new Vector3(1, 1, 1);
            }

            var desc = new PhysicsBodyDescription
            {
                Position = gameObject.Transform.Position,
                Rotation = gameObject.Transform.Rotation,
                Mass = 0f,
                IsKinematic = true,
                IsTrigger = true,
                Shape = internalShape,
                Material = Material,
                AllowSleep = allowSleep
            };

            internalBody = gameObject.Scene.Physics.CreateBody(desc, gameObject);
        }

        public override void OnUpdate(float dt)
        {
            if (internalBody == null)
                return;

            internalBody.Position = gameObject.Transform.Position;
            internalBody.Rotation = gameObject.Transform.Rotation;
        }

        public override void OnRender()
        {
            if (internalBody == null)
                return;
            Matrix4x4 model = Helper.BuildModel(internalBody!.Position, Shape.Size, internalBody!.Rotation);

            DebugRenderSystem.DrawCube(model);
        }

        public override void OnDestroy()
        {
            if (internalBody == null)
                return;
            gameObject.Scene.Physics.RemoveBody(internalBody);
        }

        // ===============================
        // Collision callbacks
        // ===============================

        public void OnCollisionEnter(GameObject other)
        {
            OnEnter?.Invoke(other);
        }

        public void OnCollisionExit(GameObject other)
        {
            OnExit?.Invoke(other);
        }

        public void OnCollisionStay(GameObject other) { }
    }
}