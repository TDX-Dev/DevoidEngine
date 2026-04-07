using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ScrollContentNode : ContainerNode
    {
        public override string ThemeType => "ScrollContentBox";

        public Vector2 ScrollOffset;

        public Vector2 ContentSize;

        protected override Vector2 MeasureCore(Vector2 available)
        {
            Vector2 measureSize = available;
            measureSize.Y = float.PositiveInfinity;

            ContentSize = base.MeasureCore(measureSize);

            return ContentSize;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            base.ArrangeCore(finalRect);

            // Offset children by scroll amount
            foreach (var child in _children)
            {
                if (!child.Visible)
                    continue;

                var rect = child.Rect;

                rect.Position.Y -= ScrollOffset.Y;

                child.Arrange(rect);
            }
        }
    }
}