using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Physics;

namespace DevoidEngine.Engine.Core
{
    public class Scene : AssetType
    {
        public event Action<Component>? OnComponentAdded;
        public event Action<Component>? OnComponentRemoved;

        public List<GameObject> GameObjects { get; private set; }

        public AudioManager Audio = null!;
        public PhysicsSystem Physics = null!;


        private bool isPlaying = false;
        private CameraComponent3D? mainCamera;
        private List<Transform3D> transforms;
        private List<CameraComponent3D> cameras;
        private List<IRenderComponent> renderables;




        public Scene()
        {
            GameObjects = new List<GameObject>();
            transforms = new List<Transform3D>();
            cameras = new List<CameraComponent3D>();
            renderables = new List<IRenderComponent>();
        }

        public void Play(bool value = true)
        {
            isPlaying = value;
            if (isPlaying)
            {
                Start();
            }
        }

        public void Start()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnStart();
            }
        }
        public void Update(float deltaTime)
        {
            //if (!isPlaying) { return; }

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(deltaTime);
            }

            if (mainCamera != null)
            {
                Audio.SetListener(mainCamera.gameObject.Transform.Position, mainCamera.gameObject.Transform.Forward, mainCamera.gameObject.Transform.Up);
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

            Physics.Step(deltaTime);
            Physics.SyncTransforms(deltaTime);
            Physics.ResolveFrameCollisions();
        }

        public void Render()
        {
           for (int i = 0;i < GameObjects.Count; i++)
            {
                GameObjects[i].OnRender();
            }
        }

        public GameObject AddGameObject(string name = "GameObject")
        {
            GameObject gameObject = new GameObject();
            gameObject.Scene = this;
            gameObject.Name = name;
            Transform3D transform = gameObject.AddComponent<Transform3D>();
            gameObject.Transform = transform;
            GameObjects.Add(gameObject);
            transforms.Add(transform);
            return gameObject;
        }

        // This method is when you already initialize your own game object.
        public GameObject AddGameObject(GameObject gameObject)
        {
            gameObject.Scene = this;
            GameObjects.Add(gameObject);
            transforms.Add(gameObject.Transform);
            return gameObject;
        }

        public void DestroyGameObject(GameObject gameObject)
        {
            transforms.Remove(gameObject.Transform);
            gameObject.OnDestroy();
            GameObjects.Remove(gameObject);
        }

        public GameObject? GetGameObject(Guid id)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                if (GameObjects[i].Id == id)
                    return GameObjects[i];
            }
            return null;
        }

        public List<T> GetComponentsOfType<T>() where T : Component
        {
            List<T> components = new List<T>();

            for (int i = 0; i < GameObjects.Count; i++)
            {
                var go = GameObjects[i];

                for (int j = 0; j < go.Components.Count; j++)
                {
                    if (go.Components[j] is T comp)
                    {
                        components.Add(comp);
                    }
                }
            }

            return components;
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
            cameras.Clear();
            renderables.Clear();
        }

        public List<IRenderComponent> GetRenderables() => renderables;
        public List<CameraComponent3D> GetCameras3D() => cameras;
        public CameraComponent3D? GetDefaultCamera3D() => mainCamera;
        public void SetMainCamera3D(CameraComponent3D camera) => mainCamera = camera;
        public void AddCamera3D(CameraComponent3D camera) => cameras.Add(camera);
        public void RemoveCamera3D(CameraComponent3D camera)
        {
            if (mainCamera ==  camera) { mainCamera = null; }
            cameras.Remove(camera);
        }

        public void ResizeCameras(int width, int height)
        {
            for (int i = 0; i <  cameras.Count; i++)
            {
                cameras[i].SetViewportSize(width, height);
            }
        }
    }
}
