using DevoidEngine.Engine.InputSystem;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public enum ScrollbarOrientation
    {
        Vertical,
        Horizontal
    }

    public class ScrollbarNode : ContainerNode
    {
        public override string ThemeType => "Scrollbar";

        public ScrollbarOrientation Orientation;

        public float ViewportSize;
        public float ContentSize;
        public float ScrollValue;

        public float Thickness = 10f;
        public float MinThumbSize = 20f;

        public ScrollbarThumb Thumb;

        float MaxScroll => Math.Max(0, ContentSize - ViewportSize);

        public ScrollbarNode()
        {
            BlockInput = true;
            Thumb = new ScrollbarThumb(this);

            base.Add(Thumb);
        }

        protected override void InitializeCore()
        {

            base.InitializeCore();
        }

        protected override Vector2 MeasureCore(Vector2 available)
        {
            if (Orientation == ScrollbarOrientation.Vertical)
                return new Vector2(Thickness, available.Y);

            return new Vector2(available.X, Thickness);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            base.ArrangeCore(finalRect);

            float trackStart;
            float trackLength;

            if (Orientation == ScrollbarOrientation.Vertical)
            {
                trackStart = Rect.Position.Y + Padding.Top;
                trackLength = Rect.Size.Y - Padding.Vertical;
            }
            else
            {
                trackStart = Rect.Position.X + Padding.Left;
                trackLength = Rect.Size.X - Padding.Horizontal;
            }

            float thumbLength = ComputeThumbLength(trackLength);
            float thumbPos = ComputeThumbPosition(trackLength, thumbLength);

            Vector2 size;
            Vector2 pos;

            if (Orientation == ScrollbarOrientation.Vertical)
            {
                size = new Vector2(Rect.Size.X - Padding.Horizontal, thumbLength);

                pos = new Vector2(
                    Rect.Position.X + Padding.Left,
                    trackStart + thumbPos
                );
            }
            else
            {
                size = new Vector2(thumbLength, Rect.Size.Y - Padding.Vertical);

                pos = new Vector2(
                    trackStart + thumbPos,
                    Rect.Position.Y + Padding.Top
                );
            }

            Thumb.Arrange(new UITransform(pos, size));
        }

        float ComputeThumbLength(float trackLength)
        {
            if (ContentSize <= 0)
                return trackLength;

            float size = trackLength * (ViewportSize / ContentSize);

            return Math.Max(MinThumbSize, size);
        }

        float ComputeThumbPosition(float trackLength, float thumbLength)
        {
            if (MaxScroll <= 0)
                return 0;

            float usableTrack = trackLength - thumbLength;

            return usableTrack * (ScrollValue / MaxScroll);
        }

        public void SetScrollFromThumb(float thumbPosition, float usableTrack)
        {
            if (usableTrack <= 0)
                return;

            float ratio = thumbPosition / usableTrack;

            ScrollValue = ratio * MaxScroll;
        }

        public override void OnMouseDown()
        {
            Vector2 mouse = UISystem.mousePosition;

            float trackStart;
            float trackLength;

            if (Orientation == ScrollbarOrientation.Vertical)
            {
                trackStart = Rect.Position.Y + Padding.Top;
                trackLength = Rect.Size.Y - Padding.Vertical;
            }
            else
            {
                trackStart = Rect.Position.X + Padding.Left;
                trackLength = Rect.Size.X - Padding.Horizontal;
            }

            float thumbLength =
                Orientation == ScrollbarOrientation.Vertical
                ? Thumb.Rect.Size.Y
                : Thumb.Rect.Size.X;

            float usableTrack = trackLength - thumbLength;

            float cursor =
                Orientation == ScrollbarOrientation.Vertical
                ? mouse.Y
                : mouse.X;

            // center the thumb on the click
            float thumbPos = cursor - trackStart - thumbLength * 0.5f;

            thumbPos = Math.Clamp(thumbPos, 0, usableTrack);

            SetScrollFromThumb(thumbPos, usableTrack);
        }
    }
}