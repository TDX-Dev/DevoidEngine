using DevoidEngine.Assets;
using DevoidEngine.Components;
using DevoidEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class Scene : AssetType, IDisposable
    {
        public event Action<Component>? OnComponentAdded;
        public event Action<Component>? OnComponentRemoved;

        public string SceneName { get; set; } = "Empty Scene";

        public List<GameObject> GameObjects { get; private set; }

        private bool isPlaying = false;
        private bool isStarted = false;


        private readonly List<Transform3D> transforms;
        private readonly List<IRenderComponent> renderables;

        public Scene()
        {
            GameObjects = [];
            transforms = [];
            renderables = [];

        }

        public void Start()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnStart();
            }
            isStarted = true;
        }

        public void Update(float deltaTime)
        {
            if (isPlaying)
            {
                for (int i = 0; i < GameObjects.Count; i++)
                {
                    GameObjects[i].OnUpdate(deltaTime);
                }
            }

            for (int i = 0; i < transforms.Count; i++)
            {
                Transform3D transform = transforms[i];

                if (!transform.hasMoved)
                    continue;

                _ = transform.WorldMatrix;

                transform.hasMoved = false;
            }
        }

        public void LateUpdate(float deltaTime)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                transforms[i].ClearDirty();
            }
        }

        public void FixedUpdate(float deltaTime)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                transforms[i].CapturePrevious();
            }

            if (!isPlaying) { return; }

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnFixedUpdate(deltaTime);
            }

            if (Engine.Instance.SimulatePhysics) { }
        }

        public void Render()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnRender();
            }
        }

        public void Play(bool value = true)
        {
            isPlaying = value;
            if (isStarted)
                throw new InvalidOperationException("Scene cannot be played before it is started.");
        }

        public GameObject AddGameObject(string name = "GameObject")
        {
            GameObject gameObject = new()
            {
                Scene = this,
                Name = name
            };
            GameObjects.Add(gameObject);
            transforms.Add(gameObject.Transform);
            return gameObject;
        }

        public void ComponentAdded(Component component)
        {
            if (component is IRenderComponent renderComponent)
                renderables.Add(renderComponent);

            if (isPlaying)
                component.OnStart();

            OnComponentAdded?.Invoke(component);
        }

        public void ComponentRemoved(Component component)
        {
            if (component is IRenderComponent renderComponent)
                renderables.Remove(renderComponent);


            OnComponentRemoved?.Invoke(component);
        }

        public void Dispose()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnDestroy();
            }

            GameObjects.Clear();
            transforms.Clear();
            renderables.Clear();
        }
    }
}
