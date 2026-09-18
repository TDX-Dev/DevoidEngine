using DevoidEngine.Attributes;
using DevoidEngine.Core;

namespace DevoidEngine.Nodes
{
    [DevoidClass]
    public class Node
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public virtual NodeTickMode TickMode { get; set; } = NodeTickMode.Play;
        public bool IsInitialized;

        public string Name = string.Empty;
        public bool Active = true;

        public Scene Scene
        {
            get => scene; 
            set
            {
                scene = value;
                SetSceneInChildren(scene);
            }
        }

        public Node? Parent;
        public List<Node> Children;

        private Scene scene = null!;

        public override string ToString() => $"Node {Name}";

        public Node()
        {
            Children = [];
        }

        public void Attach() { OnAttach(); }
        public void Destroy()
        {
            OnDestroy();
            IsInitialized = false;

            for (int i = 0; i < Children.Count; i++)
                Children[i].Destroy();
        }
        public void Start()
        {
            if (IsInitialized)
                return;
            IsInitialized = true;
            OnStart();

            for (int i = 0; i < Children.Count; i++)
                Children[i].Start();
        }
        public void Update(float dt)
        {
            OnUpdate(dt);

            for (int i = 0; i < Children.Count; i++)
                Children[i].Update(dt);
        }
        public void FixedUpdate(float dt)
        {
            OnFixedUpdate(dt);

            for (int i = 0; i < Children.Count; i++)
                Children[i].FixedUpdate(dt);
        }
        public void Render()
        {
            OnRender();

            for (int i = 0; i < Children.Count; i++)
                Children[i].Render();
        }

        protected virtual void OnAttach() { }
        protected virtual void OnStart() { }
        protected virtual void OnUpdate(float dt) { }
        protected virtual void OnFixedUpdate(float dt) { }
        protected virtual void OnRender() { }

        public virtual void SetParent(Node? node, bool keepWorldTransform = false)
        {
            if (Parent == node) // Skip if already child of node.
                return;

            Parent?.Children.Remove(this); // remove as child from prior parent, if any
            Parent = node;
            Parent?.Children.Add(this);
        }

        protected virtual void OnDestroy()
        {
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSceneInChildren(Scene scene)
        {
            // Now, this could be a part of the property setter itself, but 
            // according to a certain someone, its better discipline to keep setters that modify class
            // values.
            // i do wonder the miniscule performance benefit of inlining this.
            // while a gross oversimplification of how the CPU executes machine code,
            // by inlining this method, we can avoid another jump function
            // im sure the compiler probably inlines this anyway without manual intervention.
            // holy overthinking
            for (int i = 0; i < Children.Count; i++)
                Children[i].Scene = scene;
        }

        public virtual void InitializeTransforms() { }
        public virtual void CapturePreviousTransform() { }
        public virtual void ClearTransform() { }
    }
}
