using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public abstract class UINode
    {
        public UIContext Context = null!;

        public bool AnimateLayout = true;
        public bool BlockInput = false;
        public bool Visible = true;
        public bool Interactable = true;
        public bool ParticipatesInLayout = true;

        public Rect Rect;
        public Rect VisualRect;

        public Vector2 DesiredSize { get; private set; }
        public Vector2 Offset = Vector2.Zero;
        public Vector2? Size;
        public Vector2 MinSize = Vector2.Zero;
        public Vector2 MaxSize = new(float.PositiveInfinity);


        bool _initialized = false;

        internal UINode? _parent;
        internal readonly List<UINode> _children = [];

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

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

        protected abstract void InitializeCore();
        protected abstract Vector2 MeasureCore(Vector2 availableSize);
        protected abstract void ArrangeCore(Rect finalRect);
        //protected abstract void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order);
        protected abstract void UpdateCore(float deltaTime);

        public virtual void Dispose()
        {

        }

        public virtual void OnDragStart(Vector2 mouse) { }
        public virtual void OnDrag(Vector2 mouse, Vector2 delta) { }
        public virtual void OnDragEnd(Vector2 mouse) { }
    }
}
