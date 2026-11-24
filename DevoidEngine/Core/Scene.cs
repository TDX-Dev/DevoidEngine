using DevoidEngine.Nodes;
using DevoidEngine.Util;

namespace DevoidEngine.Core
{
    public enum SceneMode
    {
        None,
        Play,
        Edit
    }

    public sealed class Scene : IDisposable
    {
        public event Action<Node>? OnNodeAdded;
        public event Action<Node>? OnNodeRemoved;

        public string SceneName { get; set; } = "Empty Scene";
        public bool IsPlaying => SceneMode == SceneMode.Play;
        public bool IsEditing => SceneMode == SceneMode.Edit;
        public bool IsRunning => SceneMode is SceneMode.Play or SceneMode.Edit;
        public SceneMode SceneMode { get; private set; } = SceneMode.None;

        public SparseListSet<Node> nodes;
        public Node Root;

        private readonly SceneTree sceneTree;

        private int nextId = 0;
        private bool inTree = false;

        public Scene(SceneTree tree)
        {
            nodes = new();
            sceneTree = tree;

            Root = new Node
            {
                Name = "SceneRoot",
                Scene = this,
                Handle = -1
            };
        }

        public void OnTreeEntered()
        {
            inTree = true;
            Root.TreeEntered();
        }

        public void OnTreeExited()
        {
            inTree = false;
            Root.TreeExited();
        }

        public void SetMode(SceneMode mode)
        {
            if (SceneMode == mode)
                return;

            SceneMode = mode;

            if (mode == SceneMode.None)
                return;

            List<Node> nodeList = nodes.GetDenseList();

            for (int i = 0; i < nodeList.Count; i++)
            {
                nodeList[i].InitializeTransforms();
            }

            Root.Start();
        }

        public T CreateNode<T>() where T : Node, new()
        {
            T node = new()
            {
                Id = Guid.NewGuid(),
                Handle = nextId,
                CreationTick = sceneTree.TickCount,
            };

            nodes.Add(nextId++, node);
            node.SetParent(Root, true);
            OnNodeAdded?.Invoke(node);

            if (inTree)
                node.TreeEntered();

            if (IsRunning)
            {
                node.Start();
                node.InitializeTransforms();
            }

            return node;
        }

        public void RemoveNode(Node node)
        {
            if (!nodes.Remove(node.Handle))
                return;

            OnNodeRemoved?.Invoke(node);
        }
        public void Tick(float dt)
        {
            if (!IsRunning)
                return;

            Root.Tick(dt);
        }

        public void TickLate()
        {
            if (!IsRunning)
                return;


            List<Node> nodeList = nodes.GetDenseList();

            for (int i = 0; i < nodeList.Count; i++)
            {
                if (nodeList[i] is Node3D node3D)
                {
                    node3D.Transform.ClearDirty();
                }
            }
        }

        public void TickFixed(float dt)
        {
            for (int i = 0; i < nodes.GetDenseList().Count; i++)
                nodes.GetDenseList()[i].CapturePreviousTransform();

            if (!IsRunning)
                return;

            Root.TickFixed(dt);
        }
        public void Dispose()
        {
            Root.Destroy();
        }
    }
}
