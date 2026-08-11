using DevoidEngine.Core;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.UI.Theme;
using DevoidEngine.UI.Theme.Styleboxes;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.UI
{
    public abstract class UINode
    {
        public virtual string ThemeType => "Control";

        public UIContext Context = null!;

        public bool AnimateLayout = false;
        public bool BlockInput = false;
        public bool Visible = true;
        public bool Interactable = true;
        public bool ParticipatesInLayout = true;

        public Rect Rect;
        public Rect VisualRect;

        public SizeMode WidthMode = SizeMode.Stretch;
        public SizeMode HeightMode = SizeMode.Stretch;


        public Vector2 DesiredSize { get; private set; }
        public Vector2 Offset = Vector2.Zero;
        public Vector2? Size;
        public Vector2 MinSize = Vector2.Zero;
        public Vector2 MaxSize = new(float.PositiveInfinity);

        public Vector2 Pivot = Vector2.Zero;
        public float Rotation = 0f;

        public LayoutOptions Layout { get; set; } = new();

        readonly Dictionary<string, Vector4> colorOverrides = [];
        readonly Dictionary<string, object> constantOverrides = [];
        //readonly Dictionary<string, string> fontOverrides = new();
        readonly Dictionary<string, int> fontSizeOverrides = [];
        readonly Dictionary<string, Texture> iconOverrides = [];
        readonly Dictionary<string, StyleBox> styleboxOverrides = [];

        public Action? OnNodeMouseDown;
        public Action? OnNodeMouseUp;
        public Action? OnNodeMouseEnter;
        public Action? OnNodeMouseLeave;
        public Action? OnNodeMouseHeld;
        public Action<Vector2>? OnNodeMouseScroll;

        public UITheme? Theme;
        public UIState State;

        public MaterialInstance? Material { get; set; }


        private bool _initialized = false;
        private UITheme? cachedTheme;

        internal UINode? _parent;
        internal readonly List<UINode> _children = [];
        internal readonly List<UINode> _layoutChildren = [];

        internal LayoutDirtyFlags LayoutDirty =
            LayoutDirtyFlags.Measure |
            LayoutDirtyFlags.Arrange |
            LayoutDirtyFlags.Children;

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            RegisterTheme();
            InitializeCore();
            ApplyTheme();

            foreach (var child in _children)
                child.Initialize();
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

        public void Arrange(Rect finalRect)
        {
            if (!Visible)
                return;

            if (!ParticipatesInLayout && !Size.HasValue)
            {
                Vector2 desired = Measure(new(float.PositiveInfinity));
                finalRect = new Rect(finalRect.Position, desired);
            }

            Vector2 size = finalRect.Size;

            if (!Size.HasValue)
            {
                if (WidthMode == SizeMode.FitContent)
                    size.X = DesiredSize.X;

                if (HeightMode == SizeMode.FitContent)
                    size.Y = DesiredSize.Y;
            }

            size.X = Math.Clamp(size.X, MinSize.X, MaxSize.X);
            size.Y = Math.Clamp(size.Y, MinSize.Y, MaxSize.Y);

            finalRect = new Rect(finalRect.Position, size);

            bool firstLayout = Rect.Size == Vector2.Zero && Rect.Position == Vector2.Zero;

            Rect = finalRect;

            if (firstLayout)
                VisualRect = finalRect;

            ArrangeCore(Rect);
        }

        public void Update(float dt)
        {
            if (AnimateLayout)
            {
                float t = 1 - MathF.Exp(-25f * dt);

                VisualRect.Position = Vector2.Lerp(VisualRect.Position, Rect.Position, t);
                VisualRect.Size = Vector2.Lerp(VisualRect.Size, Rect.Size, t);
            }
            else
            {
                VisualRect = Rect;
            }

            UpdateCore(dt);

            foreach (var child in _children)
                child.Update(dt);
        }

        public void Render(UIDrawList drawList, int order)
        {
            if (!Visible)
                return;

            PreRender(drawList, ++order);

            foreach (var child in _children)
                child.Render(drawList, order);

            PostRender(drawList);
        }

        public virtual void Add(UINode child)
        {
            child._parent = this;
            child.Context = Context;
            _children.Add(child);

            if (_initialized)
                child.Initialize();
        }

        public virtual void Remove(UINode child)
        {
            _children.Remove(child);
        }

        public void Clear()
        {
            foreach (var child in _children)
                child.Dispose();

            _children.Clear();
        }

        protected List<UINode> GetLayoutChildren()
        {
            _layoutChildren.Clear();

            foreach (var child in _children)
            {
                if (child.Visible && child.ParticipatesInLayout)
                    _layoutChildren.Add(child);
            }

            return _layoutChildren;
        }
        public UITheme GetTheme()
        {
            if (Theme != null)
                return Theme;

            if (_parent != null)
                return _parent.GetTheme();

            return Engine.UISystem.DefaultTheme;
        }

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
                if (s != null) return s;
            }

            if (State.HasFlag(UIState.Hover))
            {
                var s = GetStyleBox(StyleKeys.Hover);
                if (s != null) return s;
            }

            if (State.HasFlag(UIState.Editing))
            {
                var s = GetStyleBox(StyleKeys.Editing);
                if (s != null) return s;
            }

            return GetStyleBox(StyleKeys.Normal);
        }

        public StyleBox? GetStyleBox(string name)
        {
            if (styleboxOverrides.TryGetValue(name, out var value))
                return value;

            return GetTheme().GetStyleBox(name, ThemeType);
        }

        //public FontInternal? GetFont(string name)
        //{
        //    if (fontOverrides.TryGetValue(name, out var value))
        //        return value;

        //    return GetTheme().GetFont(name, ThemeType);
        //}

        public int GetFontSize(string name)
        {
            if (fontSizeOverrides.TryGetValue(name, out var value))
                return value;

            return GetTheme().GetFontSize(name, ThemeType);
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

        void OnThemeChanged()
        {
            ApplyTheme();

            foreach (var child in _children)
                child.OnThemeChanged();
        }
        void RegisterTheme()
        {
            cachedTheme = GetTheme();

            if (cachedTheme != null)
                cachedTheme.ThemeChanged += OnThemeChanged;
        }
        protected abstract void InitializeCore();
        protected abstract Vector2 MeasureCore(Vector2 availableSize);
        protected abstract void ArrangeCore(Rect finalRect);
        //protected abstract void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order);
        protected abstract void UpdateCore(float deltaTime);

        protected abstract void PreRender(UIDrawList drawList, int order);
        protected abstract void PostRender(UIDrawList drawList);

        public virtual void Dispose()
        {

        }

        protected virtual void ApplyTheme() { }
        protected virtual void UpdateMaterial() { }
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
