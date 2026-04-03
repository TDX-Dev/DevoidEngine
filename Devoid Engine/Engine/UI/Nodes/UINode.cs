using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem.InputDevices;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public abstract class UINode
    {

        public int DEBUG_NUM_LOCAL = 0;
        static int DEBUG_NUM_STATIC = 0;

        public bool AnimateLayout = true;
        public bool BlockInput = false;
        public bool Visible = true;
        public bool Interactable = true;
        public bool ParticipatesInLayout = true;

        private bool _materialDirty = true;
        private bool _initialized = false;

        internal UINode? _parent;
        internal List<UINode> _children = new();


        public UITransform Rect { get; protected set; } = default!;
        public UITransform VisualRect { get; protected set; } = default!;
        public Vector2 DesiredSize { get; set; }

        public LayoutOptions Layout { get; set; } = new LayoutOptions();

        public Vector2 Offset = Vector2.Zero;
        public Vector2? Size;
        public Vector2 MinSize = Vector2.Zero;
        public Vector2 MaxSize = new(float.PositiveInfinity);

        public Vector2 Pivot = Vector2.Zero;
        public float Rotation = 0f;


        public MaterialInstance? Material { get; set; }

        public UITheme? Theme;
        private UITheme? cachedTheme;

        public UIState State;

        Dictionary<string, Vector4> colorOverrides = new();
        Dictionary<string, object> constantOverrides = new();
        Dictionary<string, FontInternal> fontOverrides = new();
        Dictionary<string, int> fontSizeOverrides = new();
        Dictionary<string, Texture2D> iconOverrides = new();
        Dictionary<string, StyleBox> styleboxOverrides = new();


        public Action? OnNodeMouseDown;
        public Action? OnNodeMouseUp;
        public Action? OnNodeMouseEnter;
        public Action? OnNodeMouseLeave;
        public Action? OnNodeMouseHeld;
        public Action<Vector2>? OnNodeMouseScroll;


        public virtual string ThemeType => "Control";
        public UINode? Parent => _parent;
        public IReadOnlyList<UINode> Children => _children;


        public UINode()
        {
            DEBUG_NUM_LOCAL = DEBUG_NUM_STATIC++;
        }

        public virtual void Add(UINode child)
        {
            child._parent = this;
            _children.Add(child);

            // if this node is already initialized, initialize the child immediately
            if (_initialized)
                child.Initialize();
        }

        public virtual void Remove(UINode child)
        {
            _children.Remove(child);
        }

        public void Clear()
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Dispose();
            }

            _children.Clear();
        }


        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            RegisterTheme();
            InitializeCore();
            ApplyTheme();

            for (int i = 0; i < _children.Count; i++)
                _children[i].Initialize();
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


        public UITheme GetTheme()
        {
            if (Theme != null)
                return Theme;

            if (Parent != null)
                return Parent.GetTheme();

            return UISystem.DefaultTheme!;
        }

        protected virtual void ApplyTheme() { }
        protected virtual void UpdateMaterial() { }

        //public Vector4 GetColor(string name)
        //{
        //    if (colorOverrides.TryGetValue(name, out var value))
        //        return value;

        //    var theme = GetTheme();

        //    if (theme.HasColor(name, ThemeType))
        //        return theme.GetColor(name, ThemeType);

        //    return Vector4.One;
        //}

        protected Vector4 GetStateColor(string property)
        {
            var theme = GetTheme();

            if (State.HasFlag(UIState.Pressed) &&
                theme.HasColor(property + "_" + StyleKeys.Pressed, ThemeType))
                return GetColor(property + "_" + StyleKeys.Pressed);

            if (State.HasFlag(UIState.Hover) &&
                theme.HasColor(property + "_" + StyleKeys.Hover, ThemeType))
                return GetColor(property + "_" + StyleKeys.Hover);

            return GetColor(property);
        }

        public Vector4 GetColor(string name)
        {
            if (colorOverrides.TryGetValue(name, out var value))
                return value;

            return GetTheme().GetColor(name, ThemeType);
        }

        public T GetConstant<T>(string name) where T : struct
        {
            if (constantOverrides.TryGetValue(name, out var value))
                return (T)value;

            var theme = GetTheme();

            if (theme.HasConstant(name, ThemeType))
                return theme.GetConstant<T>(name, ThemeType);

            return default;
        }

        protected StyleBox? GetStateStyleBox()
        {
            if (State.HasFlag(UIState.Pressed))
            {
                var s = GetStyleBox(StyleKeys.Pressed);
                if (s != null)
                    return s;
            }

            if (State.HasFlag(UIState.Hover))
            {
                var s = GetStyleBox(StyleKeys.Hover);
                if (s != null)
                    return s;
            }

            if (State.HasFlag(UIState.Editing))
            {
                var s = GetStyleBox(StyleKeys.Editing);
                if (s != null)
                    return s;
            }

            return GetStyleBox(StyleKeys.Normal);
        }

        public StyleBox? GetStyleBox(string name)
        {
            if (styleboxOverrides.TryGetValue(name, out var value))
                return value;

            return GetTheme().GetStyleBox(name, ThemeType);
        }

        public FontInternal? GetFont(string name)
        {
            if (fontOverrides.TryGetValue(name, out var value))
                return value;

            var theme = GetTheme();
            return theme.GetFont(name, ThemeType);
        }

        public int GetFontSize(string name)
        {
            if (fontSizeOverrides.TryGetValue(name, out var value))
                return value;

            var theme = GetTheme();

            return theme.GetFontSize(name, ThemeType);
        }


        public void AddColorOverride(string name, Vector4 value)
        {
            colorOverrides[name] = value;
            OnThemeChanged();
        }

        public void AddConstantOverride(string name, object value)
        {
            constantOverrides[name] = value;
            OnThemeChanged();
        }

        public void AddStyleBoxOverride(string name, StyleBox style)
        {
            styleboxOverrides[name] = style;
            OnThemeChanged();
        }

        public void RemoveColorOverride(string name)
        {
            colorOverrides.Remove(name);
            OnThemeChanged();
        }


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

        public void Arrange(UITransform finalRect)
        {
            if (!Visible)
                return;

            if (!ParticipatesInLayout && !Size.HasValue)
            {
                Vector2 desired = Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                finalRect = new UITransform(finalRect.position, desired);
            }

            Rect = finalRect;

            if (VisualRect == null)
                VisualRect = finalRect;

            ArrangeCore(Rect);
        }


        public virtual void Render(List<RenderItem> renderList, Matrix4x4 model, int order)
        {
            if (!Visible || VisualRect == null || Rect == null)
                return;

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

        public void Update(float dt)
        {
            if (VisualRect == null)
                return;

            if (AnimateLayout)
            {
                float speed = 25f;

                VisualRect.position = Vector2.Lerp(
                    VisualRect.position,
                    Rect?.position ?? Vector2.Zero,
                    1 - MathF.Exp(-speed * dt)
                );

                VisualRect.size = Vector2.Lerp(
                    VisualRect.size,
                    Rect!.size,
                    1 - MathF.Exp(-speed * dt)
                );
            } else
            {
                VisualRect = Rect;
            }

                UpdateCore(dt);

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Update(dt);
            }
        }

        protected abstract void InitializeCore();
        protected abstract Vector2 MeasureCore(Vector2 availableSize);
        protected abstract void ArrangeCore(UITransform finalRect);
        protected abstract void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order);
        protected abstract void UpdateCore(float deltaTime);


        public virtual void Dispose() { }


        public virtual void OnDragStart(Vector2 mouse) { }
        public virtual void OnDrag(Vector2 mouse, Vector2 delta) { }
        public virtual void OnDragEnd(Vector2 mouse) { }

        public virtual void OnMouseEnter() => OnNodeMouseEnter?.Invoke();
        public virtual void OnMouseLeave() => OnNodeMouseLeave?.Invoke();
        public virtual void OnMouseHeld() => OnNodeMouseHeld?.Invoke();

        public virtual void OnMouseDown() => OnNodeMouseDown?.Invoke();
        public virtual void OnMouseUp() => OnNodeMouseUp?.Invoke();
        public virtual void OnMouseScroll(Vector2 scroll) => OnNodeMouseScroll?.Invoke(scroll);

        public virtual void OnKeyDown(Keys key) { }
        public virtual void OnKeyUp(Keys key) { }
        public virtual void OnKeyPressed(Keys key) { }
        public virtual void OnTextInput(char value) { }

        public virtual void OnClick() { }

        public virtual void OnFocus() { }
        public virtual void OnBlur() { }
    }
}