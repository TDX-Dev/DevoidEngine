using DevoidEngine.Assets;
using DevoidEngine.Audio;
using DevoidEngine.Nodes;
using DevoidEngine.Physics;
using DevoidEngine.Rendering;
using System.ComponentModel;

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
        public event Action<Node>? OnNodeAdded;
        public event Action<Node>? OnNodeRemoved;

        public string SceneName { get; set; } = "Empty Scene";
        public bool IsPlaying => SceneMode == SceneMode.Play;
        public bool IsEditing => SceneMode == SceneMode.Edit;
        public bool IsRunning => SceneMode is SceneMode.Play or SceneMode.Edit;
        public SceneMode SceneMode { get; private set; } = SceneMode.None;

        public List<Node> Nodes { get; private set; }
        public RenderWorld World { get; private set; } = null!;
        public PhysicsSystem Physics { get; internal set; } = null!;
        public AudioManager Audio { get; internal set; } = null!;

        public List<Camera3D> Cameras { get; private set; } = [];
        public Camera3D? MainCamera { get; private set; }

        private readonly List<Transform3D> transforms;

        public Scene()
        {
            Nodes = [];
            transforms = [];

            World = new();
        }

        public void Update(float deltaTime)
        {
            if (!IsRunning)
                return;

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Parent != null)
                    continue;

                Nodes[i].Update(deltaTime);
            }

            if (MainCamera != null)
            {
                Transform3D camTransform = MainCamera.Transform;

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

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Parent != null)
                    continue;

                Nodes[i].FixedUpdate(deltaTime);
            }

            // Physics simulation itself remains Play-only.
            if (IsPlaying && Engine.Instance.SimulatePhysics)
            {
                Physics.Step(1 / Engine.Instance.TargetFramerate);
                Physics.SyncTransforms(1 / Engine.Instance.TargetFramerate);
                Physics.ResolveFrameCollisions();
            }
        }

        public void Render()
        {
            if (!IsRunning)
                return;

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Parent != null)
                    continue;

                Nodes[i].Render();
            }
        }

        public void SetMode(SceneMode mode)
        {
            if (SceneMode == mode)
                return;

            SceneMode = mode;

            if (mode == SceneMode.None)
                return;

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Parent != null)
                    continue;

                Nodes[i].Start();
            }

            for (int i = 0; i < transforms.Count; i++)
            {
                transforms[i].InitializeInterpolation();
            }
        }

        public T CreateNode<T>(string name = "Node") where T : Node, new()
        {
            T node = new()
            {
                Scene = this,
                Name = name
            };

            Nodes.Add(node);

            if (node is Node3D node3D)
                transforms.Add(node3D.Transform);

            node.Attach();

            if (IsRunning)
            {
                node.Start();
                node.InitializeTransforms();
            }

            return node;
        }

        public Node AddNode(Node node)
        {
            Nodes.Add(node);

            if (node is Node3D node3D)
                transforms.Add(node3D.Transform);

            node.Attach();

            if (IsRunning)
            {
                node.Start();
                node.InitializeTransforms();
            }

            return node;
        }

        public Node? GetNode(Guid id)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Id == id)
                    return Nodes[i];
            }

            return null;
        }

        public Node? GetNode(string name)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Name == name)
                    return Nodes[i];
            }

            return null;
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
        public void NodeAdded(Node node)
        {
            OnNodeAdded?.Invoke(node);
        }

        public void NodeRemoved(Node node)
        {
            OnNodeRemoved?.Invoke(node);
        }
        public override void Dispose()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Parent != null)
                    continue;

                Nodes[i].Destroy();
            }

            Nodes.Clear();
            transforms.Clear();
            Cameras.Clear();
            MainCamera = null;
        }
    }
}