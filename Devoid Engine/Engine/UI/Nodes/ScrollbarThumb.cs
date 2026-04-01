using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class ScrollbarThumb : ContainerNode
    {
        public override string ThemeType => "ScrollbarThumb";

        ScrollbarNode scrollbar;

        float dragStartThumb;
        float dragStartMouse;

        public ScrollbarThumb(ScrollbarNode owner)
        {
            scrollbar = owner;
            BlockInput = true;   // REQUIRED
        }

        public override void OnDragStart(Vector2 mouse)
        {
            Console.WriteLine("Draggin!");

            if (scrollbar.Orientation == ScrollbarOrientation.Vertical)
            {
                dragStartMouse = mouse.Y;
                dragStartThumb = Rect.position.Y;
            }
            else
            {
                dragStartMouse = mouse.X;
                dragStartThumb = Rect.position.X;
            }
        }

        public override void OnDrag(Vector2 mouse, Vector2 delta)
        {
            float cursor =
                scrollbar.Orientation == ScrollbarOrientation.Vertical
                ? mouse.Y
                : mouse.X;

            float trackStart =
                scrollbar.Orientation == ScrollbarOrientation.Vertical
                ? scrollbar.Rect.position.Y + scrollbar.Padding.Top
                : scrollbar.Rect.position.X + scrollbar.Padding.Left;

            float trackLength =
                scrollbar.Orientation == ScrollbarOrientation.Vertical
                ? scrollbar.Rect.size.Y - scrollbar.Padding.Vertical
                : scrollbar.Rect.size.X - scrollbar.Padding.Horizontal;

            float thumbLength =
                scrollbar.Orientation == ScrollbarOrientation.Vertical
                ? Rect.size.Y
                : Rect.size.X;

            float usableTrack = trackLength - thumbLength;

            float thumbPos = dragStartThumb + (cursor - dragStartMouse) - trackStart;
            thumbPos = Math.Clamp(thumbPos, 0, usableTrack);

            scrollbar.SetScrollFromThumb(thumbPos, usableTrack);
        }

        public override void OnDragEnd(Vector2 mouse)
        {
        }
    }
}