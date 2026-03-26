using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ScrollNode : FlexboxNode
    {
        public Vector2 ScrollOffset = Vector2.Zero;

        private Vector2 contentSize;

        public float ScrollSpeed = 30f;

        protected override void InitializeCore()
        {
            BlockInput = true;
        }

        protected override Vector2 MeasureCore(Vector2 available)
        {
            contentSize = base.MeasureCore(available);

            // viewport size is fixed
            return Size ?? available;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            // normal flex layout
            base.ArrangeCore(finalRect);

            // shift children by scroll offset
            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];

                child.Rect.position -= ScrollOffset;
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



            //DebugRenderSystem.DrawRectUI(
            //    UISystem.BuildModel(Rect)    
            //);

            base.Render(renderList, canvas, order);

            UIScissorStack.Pop();
        }

        public void ScrollToBottom()
        {
            float maxScroll = Math.Max(0, contentSize.Y - Rect.size.Y);
            ScrollOffset.Y = maxScroll;
        }

        //public override void OnMouseWheel(float delta)
        //{
        //    ScrollOffset.Y -= delta * ScrollSpeed;

        //    float maxScroll = Math.Max(0, contentSize.Y - Rect.size.Y);

        //    ScrollOffset.Y = Math.Clamp(ScrollOffset.Y, 0, maxScroll);
        //}

    }
}