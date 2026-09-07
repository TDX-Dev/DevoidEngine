using Assimp;
using Assimp.Unmanaged;
using DevoidEngine.Assets;
using DevoidEngine.Audio;
using DevoidEngine.Components;
using DevoidEngine.Physics;
using DevoidEngine.Rendering;

namespace DevoidEngine.Core
{
    public enum SceneMode
    {
        None,
        Play,
        Edit
    }

    public sealed class Scene : AssetType, IDisposable
    {
        public event Action<Component>? OnComponentAdded;
        public event Action<Component>? OnComponentRemoved;

        public string SceneName { get; set; } = "Empty Scene";
        public bool IsPlaying => SceneMode == SceneMode.Play;
        public bool IsEditing => SceneMode == SceneMode.Edit;
        public bool IsRunning => SceneMode is SceneMode.Play or SceneMode.Edit;
        public SceneMode SceneMode { get; private set; } = SceneMode.None;

        public List<GameObject> GameObjects { get; private set; }
        public RenderWorld World { get; private set; } = null!;
        public PhysicsSystem Physics { get; internal set; } = null!;
        public AudioManager Audio { get; internal set; } = null!;

        public List<Camera3D> Cameras { get; private set; } = [];
        public Camera3D? MainCamera { get; private set; }


        private readonly List<Transform3D> transforms;
        private readonly List<IRenderComponent> renderables;

        public Scene()
        {
            GameObjects = [];
            transforms = [];
            renderables = [];

            World = new();
        }

        public void Update(float deltaTime)
        {
            if (!IsRunning)
                return;

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(deltaTime);
            }

            //for (int i = 0; i < transforms.Count; i++)
            //{
            //    Transform3D transform = transforms[i];

            //    if (!transform.hasMoved)
            //        continue;

            //    _ = transform.WorldMatrix;
            //}

            if (MainCamera != null)
            {
                Transform3D camTransform = MainCamera.gameObject.Transform;

                Audio?.SetListener(
                    camTransform.Position,
                    camTransform.Forward,
                    camTransform.Up);
            }

            World.UpdateAccelerationStructures();
        }

        public void LateUpdate(float deltaTime)
        {
            if (!IsRunning)
                return;

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

            if (!IsRunning)
                return;

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnFixedUpdate(deltaTime);
            }

            // Physics simulation itself remains Play-only.
            if (IsPlaying && Engine.Instance.SimulatePhysics)
            {
                Physics.Step(/*deltaTime*/1/Engine.Instance.TargetFramerate);
                Physics.SyncTransforms(/*deltaTime*/ 1 / Engine.Instance.TargetFramerate);
                Physics.ResolveFrameCollisions();
            }
        }

        public void Render()
        {
            if (!IsRunning)
                return;

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnRender();
            }
        }

        public void SetMode(SceneMode mode)
        {
            if (SceneMode == mode)
                return;

            SceneMode = mode;

            if (mode == SceneMode.None)
                return;

            // Let components decide whether they should start
            // based on the new scene mode.
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnStart();
            }
            foreach (var transform in transforms)
            {
                transform.InitializeInterpolation();
            }
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

            if (IsRunning)
            {
                gameObject.OnStart();
                gameObject.Transform.InitializeInterpolation();
            }

            return gameObject;
        }

        public GameObject AddGameObject(GameObject gameObject)
        {
            gameObject.Scene = this;

            GameObjects.Add(gameObject);
            transforms.Add(gameObject.Transform);

            if (IsRunning)
            {
                gameObject.OnStart();
                gameObject.Transform.InitializeInterpolation();
            }

            return gameObject;
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

        public GameObject? GetGameObject(string name)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                if (GameObjects[i].Name == name)
                    return GameObjects[i];
            }
            return null;
        }
        public void RemoveGameObject(GameObject gameObject)
        {
            if (!GameObjects.Contains(gameObject))
                return;

            // Copy because destroying children modifies the hierarchy.
            var children = gameObject.Children.ToArray();

            for (int i = 0; i < children.Length; i++)
            {
                RemoveGameObject(children[i]);
            }

            gameObject.SetParent(null);

            transforms.Remove(gameObject.Transform);
            GameObjects.Remove(gameObject);

            gameObject.OnDestroy();
        }

        public void RegisterCamera(Camera3D camera)
        {
            if (!Cameras.Contains(camera))
            {
                Cameras.Add(camera);
            }

            if (MainCamera == null || camera.IsCurrent)
            {
                SetMainCamera(camera);
            }
        }

        public void UnregisterCamera(Camera3D camera)
        {
            Cameras.Remove(camera);

            if (MainCamera == camera)
            {
                MainCamera = Cameras.Count > 0 ? Cameras[0] : null;
            }
        }

        public void SetMainCamera(Camera3D camera)
        {
            MainCamera = camera;
            foreach (var cam in Cameras)
            {
                cam.SetIsCurrentInternal(cam == camera);
            }
        }
        public void ComponentAdded(Component component)
        {
            if (component is IRenderComponent renderComponent)
                renderables.Add(renderComponent);

            if (IsRunning)
                component.InternalStart();

            OnComponentAdded?.Invoke(component);
        }

        public void ComponentRemoved(Component component)
        {
            if (component is IRenderComponent renderComponent)
                renderables.Remove(renderComponent);


            OnComponentRemoved?.Invoke(component);
        }

        public override void Dispose()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnDestroy();
            }

            GameObjects.Clear();
            transforms.Clear();
            renderables.Clear();
            Cameras.Clear();
            MainCamera = null;
        }
    }
}
