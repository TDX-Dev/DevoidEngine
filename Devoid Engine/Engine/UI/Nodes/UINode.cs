using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Theme;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public abstract class UINode
    {
        // REMOVE
        public int DEBUG_NUM_LOCAL = 0;
        static int DEBUG_NUM_STATIC = 0;

        public UINode()
        {
            DEBUG_NUM_LOCAL = DEBUG_NUM_STATIC++;
        }

        public bool BlockInput = false;

        public virtual string ThemeType => "Control";
        public UINode Parent => _parent;
        public IReadOnlyList<UINode> Children => _children;

        public UITransform Rect { get; protected set; }
        public Vector2 DesiredSize { get; set; }
        public LayoutOptions Layout { get; set; } = new LayoutOptions();
        public MaterialInstance Material { get; set; }

        public bool Visible = true;
        public bool Interactable = true;
        private bool _materialDirty = true;

        public UITheme Theme;
        private UITheme cachedTheme;

        internal UINode _parent;
        internal List<UINode> _children = new();


        // Size intent
        public Vector2 Offset = Vector2.Zero;
        public Vector2? Size;
        public Vector2 MinSize = Vector2.Zero;
        public Vector2 MaxSize = new(float.PositiveInfinity);
        public Vector2 Pivot = Vector2.Zero;
        public float Rotation = 0f;

        public bool ParticipatesInLayout = true;

        public Action OnNodeMouseDown;
        public Action OnNodeMouseUp;
        public Action OnNodeMouseEnter;
        public Action OnNodeMouseLeave;
        public Action OnNodeMouseHeld;

        public UITheme GetTheme()
        {
            if (Theme != null)
                return Theme;

            if (Parent != null)
                return Parent.GetTheme();

            return UISystem.DefaultTheme;
        }

        public void Add(UINode child)
        {
            child._parent = this;
            _children.Add(child);
            child.Initialize();
        }

        public void Initialize()
        {
            RegisterTheme();
            InitializeCore();
            ApplyTheme();
        }

        void RegisterTheme()
        {
            cachedTheme = GetTheme();

            if (cachedTheme != null)
                cachedTheme.ThemeChanged += OnThemeChanged;
        }

        void OnThemeChanged()
        {
            ApplyTheme();

            for (int i = 0; i < _children.Count; i++)
                _children[i].OnThemeChanged();
        }

        protected virtual void ApplyTheme() { }
        protected virtual void UpdateMaterial() { }

        // ENTRY POINT
        public Vector2 Measure(Vector2 availableSize)
        {
            if (!Visible)
                return Vector2.Zero;

            Vector2 desired = MeasureCore(availableSize);

            if (Size.HasValue)
                desired = Size.Value;

            desired.X = Math.Clamp(desired.X, MinSize.X, MaxSize.X);
            desired.Y = Math.Clamp(desired.Y, MinSize.Y, MaxSize.Y);

            DesiredSize = desired;
            return desired;
        }

        //public void Arrange(UITransform finalRect)
        //{
        //    if (!Visible)
        //        return;

        //    Vector2 size = finalRect.size;

        //    // if size was not explicitly set, use measured size
        //    if (!Size.HasValue)
        //        size = DesiredSize;

        //    Rect = new UITransform(finalRect.position, size);

        //    ArrangeCore(Rect);
        //}

        //public void Arrange(UITransform finalRect)
        //{
        //    if (!Visible)
        //        return;

        //    Vector2 size = finalRect.size;

        //    if (Size.HasValue)
        //        size = Size.Value;

        //    Rect = new UITransform(finalRect.position, size);

        //    Console.Write(size);

        //    ArrangeCore(Rect);
        //}

        public void Arrange(UITransform finalRect)
        {
            if (!Visible)
                return;

            Rect = finalRect;

            ArrangeCore(Rect);
        }

        public virtual void Render(List<RenderItem> renderList, Matrix4x4 model, int order)
        {
            if (!Visible) return;
            if (_materialDirty)
            {
                UpdateMaterial();
                _materialDirty = false;
            }
            RenderCore(renderList, model, ++order);
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Render(renderList, model, order);
            }
        }

        public void Update(float deltaTime)
        {
            UpdateCore(deltaTime);
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Update(deltaTime);
            }
        }

        // OVERRIDES
        protected abstract void InitializeCore();
        protected abstract Vector2 MeasureCore(Vector2 availableSize);
        protected abstract void ArrangeCore(UITransform finalRect);
        protected abstract void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order);
        protected abstract void UpdateCore(float deltaTime);

        public virtual void Dispose() { }

        public void Clear()
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Dispose();
            }
            _children.Clear();
        }

        public virtual void OnDragStart(Vector2 mouse) { }
        public virtual void OnDrag(Vector2 mouse, Vector2 delta) { }
        public virtual void OnDragEnd(Vector2 mouse) { }

        public virtual void OnMouseEnter() => OnNodeMouseEnter?.Invoke();
        public virtual void OnMouseLeave() => OnNodeMouseLeave?.Invoke();
        public virtual void OnMouseHeld() => OnNodeMouseHeld?.Invoke();

        public virtual void OnMouseDown() => OnNodeMouseDown?.Invoke();
        public virtual void OnMouseUp() => OnNodeMouseUp?.Invoke();

        public virtual void OnClick() { }

        public virtual void OnFocus() { }
        public virtual void OnBlur() { }
    }

}