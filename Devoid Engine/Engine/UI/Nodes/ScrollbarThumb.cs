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
                dragStartThumb = Rect.Position.Y;
            }
            else
            {
                dragStartMouse = mouse.X;
                dragStartThumb = Rect.Position.X;
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
                ? scrollbar.Rect.Position.Y + scrollbar.Padding.Top
                : scrollbar.Rect.Position.X + scrollbar.Padding.Left;

            float trackLength =
                scrollbar.Orientation == ScrollbarOrientation.Vertical
                ? scrollbar.Rect.Size.Y - scrollbar.Padding.Vertical
                : scrollbar.Rect.Size.X - scrollbar.Padding.Horizontal;

            float thumbLength =
                scrollbar.Orientation == ScrollbarOrientation.Vertical
                ? Rect.Size.Y
                : Rect.Size.X;

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