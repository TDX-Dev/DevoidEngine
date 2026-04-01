using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ScrollNode : FlexboxNode
    {
        public Vector2 ScrollOffset = Vector2.Zero;
        public float ScrollSpeed = 40f;

        private ScrollContentNode InnerContainer;

        private ScrollbarNode VScrollbar;
        private ScrollbarNode HScrollbar;

        const float ScrollbarSize = 20f;

        public ScrollNode()
        {
            InnerContainer = new ScrollContentNode()
            {
                Wrap = FlexWrap.Wrap,
                Layout = new LayoutOptions()
                {
                    FlexGrowCross = 1,
                    FlexGrowMain = 0
                }
            };

            VScrollbar = new ScrollbarNode()
            {
                Orientation = ScrollbarOrientation.Vertical,
                Thickness = ScrollbarSize
            };

            HScrollbar = new ScrollbarNode()
            {
                Orientation = ScrollbarOrientation.Horizontal,
                Thickness = ScrollbarSize
            };

            BlockInput = true;
        }

        public override void Add(UINode child)
        {
            InnerContainer.Add(child);
        }

        public override void Remove(UINode child)
        {
            InnerContainer.Remove(child);
        }

        protected override void InitializeCore()
        {

            InnerContainer.Gap = Gap;
            InnerContainer.Padding = Padding;

            Padding = Padding.GetAll(0);
            Gap = 0;

            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 1;

            base.Add(InnerContainer);
            base.Add(VScrollbar);
            base.Add(HScrollbar);

            base.InitializeCore();
        }

        protected override Vector2 MeasureCore(Vector2 available)
        {
            Vector2 contentSize = InnerContainer.Measure(new Vector2(
                available.X,
                float.PositiveInfinity
            ));

            return new Vector2(
                Size?.X ?? Math.Min(contentSize.X, available.X),
                Size?.Y ?? Math.Min(contentSize.Y, available.Y)
            );
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            Vector2 viewportSize = Rect.size;

            // ensure content size is up to date
            InnerContainer.Measure(new Vector2(
                viewportSize.X,
                float.PositiveInfinity
            ));

            bool needV = InnerContainer.ContentSize.Y > viewportSize.Y;
            bool needH = InnerContainer.ContentSize.X > viewportSize.X;

            if (needV)
                viewportSize.X -= ScrollbarSize;

            if (needH)
                viewportSize.Y -= ScrollbarSize;

            // arrange content
            InnerContainer.Arrange(new UITransform(
                Rect.position,
                viewportSize
            ));

            float maxScrollY = Math.Max(0, InnerContainer.ContentSize.Y - viewportSize.Y);
            float maxScrollX = Math.Max(0, InnerContainer.ContentSize.X - viewportSize.X);

            ScrollOffset.Y = Math.Clamp(ScrollOffset.Y, 0, maxScrollY);
            ScrollOffset.X = Math.Clamp(ScrollOffset.X, 0, maxScrollX);

            InnerContainer.ScrollOffset = ScrollOffset;

            // vertical scrollbar
            VScrollbar.Visible = needV;

            if (needV)
            {
                VScrollbar.ViewportSize = viewportSize.Y;
                VScrollbar.ContentSize = InnerContainer.ContentSize.Y;
                ScrollOffset.Y = VScrollbar.ScrollValue;

                VScrollbar.Arrange(new UITransform(
                    new Vector2(Rect.position.X + viewportSize.X, Rect.position.Y),
                    new Vector2(ScrollbarSize, viewportSize.Y)
                ));

                ScrollOffset.Y = VScrollbar.ScrollValue;
            }

            // horizontal scrollbar
            HScrollbar.Visible = needH;

            if (needH)
            {
                HScrollbar.ViewportSize = viewportSize.X;
                HScrollbar.ContentSize = InnerContainer.ContentSize.X;
                ScrollOffset.X = HScrollbar.ScrollValue;

                HScrollbar.Arrange(new UITransform(
                    new Vector2(Rect.position.X, Rect.position.Y + viewportSize.Y),
                    new Vector2(viewportSize.X, ScrollbarSize)
                ));

                ScrollOffset.X = HScrollbar.ScrollValue;
            }
        }

        public override void Render(List<RenderItem> renderList, Matrix4x4 canvas, int order)
        {
            UIScissorStack.Push(
                Rect.position.X,
                Rect.position.Y,
                Rect.size.X,
                Rect.size.Y
            );

            InnerContainer.Render(renderList, canvas, order);

            //VScrollbar.Render(renderList, canvas, order + 1);
            UIScissorStack.Pop();

            if (VScrollbar.Visible)
                VScrollbar.Render(renderList, canvas, order + 1);

            if (HScrollbar.Visible)
                HScrollbar.Render(renderList, canvas, order + 1);
        }

        public override void OnMouseScroll(Vector2 scroll)
        {
            ScrollOffset.Y -= scroll.Y * ScrollSpeed;
            ScrollOffset.X -= scroll.X * ScrollSpeed;

            float maxScrollY = Math.Max(0, InnerContainer.ContentSize.Y - Rect.size.Y);
            float maxScrollX = Math.Max(0, InnerContainer.ContentSize.X - Rect.size.X);

            ScrollOffset.Y = Math.Clamp(ScrollOffset.Y, 0, maxScrollY);
            ScrollOffset.X = Math.Clamp(ScrollOffset.X, 0, maxScrollX);

            VScrollbar.ScrollValue = ScrollOffset.Y;
            HScrollbar.ScrollValue = ScrollOffset.X;
        }
    }
}