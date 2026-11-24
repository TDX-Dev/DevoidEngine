using DevoidEngine.Core;
using DevoidEngine.Rendering;

namespace DevoidEngine.Nodes
{
    public class Node
    {
        public Guid Id { get; set; }
        public int Handle = -1;
        public string Name = string.Empty;
        public virtual NodeTickMode TickMode { get; set; } = NodeTickMode.Play;

        public bool IsInitialized = false;
        public bool Active = true;
        public ulong CreationTick = 0;




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
        public void TreeEntered()
        {
            OnTreeEntered();
            for (int i = 0; i < Children.Count; i++)
                Children[i].TreeEntered();
        }
        public void TreeExited()
        {
            OnTreeExited();
            for (int i = 0; i < Children.Count; i++)
                Children[i].TreeExited();
        }

        public Viewport GetViewport()
        {
            throw new NotImplementedException();
        }
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
        public void Tick(float dt)
        {
            OnTick(dt);

            for (int i = 0; i < Children.Count; i++)
                Children[i].Tick(dt);
        }
        public void TickFixed(float dt)
        {
            OnTickFixed(dt);

            for (int i = 0; i < Children.Count; i++)
                Children[i].TickFixed(dt);
        }
        public void Render()
        {
            OnRender();

            for (int i = 0; i < Children.Count; i++)
                Children[i].Render();
        }

        protected virtual void OnTreeEntered() { }
        protected virtual void OnTreeExited() { }
        protected virtual void OnStart() { }
        protected virtual void OnTick(float dt) { }
        protected virtual void OnTickFixed(float dt) { }
        protected virtual void OnRender() { }

        public virtual void SetParent(Node? node, bool keepWorldTransform = false)
        {
            if (Parent == node)
                return;

            // Prevent cycles.
            for (Node? current = node; current != null; current = current.Parent)
            {
                if (current == this)
                    throw new InvalidOperationException("Cannot create a hierarchy cycle.");
            }

            Parent?.Children.Remove(this);

            Parent = node;

            Parent?.Children.Add(this);

            if (node != null)
                Scene = node.Scene;
        }

        public T? GetAncestor<T>() where T : Node
        {
            Node? current = Parent;

            while (current != null)
            {
                if (current is T result)
                    return result;

                current = current.Parent;
            }

            return null;
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