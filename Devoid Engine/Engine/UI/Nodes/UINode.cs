using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
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


        public UITransform Rect { get; protected set; }
        public Vector2 DesiredSize { get; private set; }
        public LayoutOptions Layout { get; set; } = new LayoutOptions();
        public MaterialInstance Material { get; set; }

        public bool Visible = true;
        public bool Interactable = true;

        protected readonly List<UINode> _children = new();



        // Size intent
        public Vector2 Offset = Vector2.Zero;
        public Vector2? Size;
        public Vector2 MinSize = Vector2.Zero;
        public Vector2 MaxSize = new(float.PositiveInfinity);

        public bool ParticipatesInLayout = true;

        public void Add(UINode child)
        {
            _children.Add(child);
            child.Initialize();
        }

        public void Initialize()
        {
            InitializeCore();
        }

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

        public void Arrange(UITransform finalRect)
        {
            Rect = finalRect;

            if (!Visible)
                return;

            ArrangeCore(finalRect);
        }
        public void Render(List<RenderItem> renderList, Matrix4x4 model)
        {
            RenderCore(renderList, model);
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].Render(renderList, model);
            }
        }

        // OVERRIDES
        protected abstract void InitializeCore();
        protected abstract Vector2 MeasureCore(Vector2 availableSize);
        protected abstract void ArrangeCore(UITransform finalRect);
        protected abstract void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel);
    }

}
