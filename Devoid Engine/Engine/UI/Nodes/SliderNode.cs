using DevoidEngine.Engine.Rendering;
using System;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    class SliderTrackNode : ContainerNode
    {
        public override string ThemeType => "SliderTrack";
    }

    class SliderThumbNode : ContainerNode
    {
        public override string ThemeType => "SliderThumb";
    }

    public class SliderNode : FlexboxNode
    {
        public override string ThemeType => "Slider";

        public float Min = 0f;
        public float Max = 1f;

        private float _value = 0f;

        public float Value
        {
            get => _value;
            set
            {
                float clamped = Math.Clamp(value, Min, Max);

                if (Math.Abs(clamped - _value) < 0.0001f)
                    return;

                _value = clamped;

                UpdateThumb();
                OnValueChanged?.Invoke(_value);
            }
        }

        public float TrackThicknessRatio = 0.35f;
        public float ThumbSizeRatio = 0.65f;
        public float MinThumbSize = 10f;

        public float Step = 0f;

        public Action<float>? OnValueChanged;

        public ContainerNode track;
        public ContainerNode thumb;

        public SliderNode()
        {
            BlockInput = true;
            MinSize = new Vector2(80, 16);

            track = new SliderTrackNode();
            thumb = new SliderThumbNode();
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            Direction = FlexDirection.Row;
            Align = AlignItems.Center;

            track.Layout.FlexGrowMain = 1;
            track.MinSize = new Vector2(60, 6);

            thumb.Size = new Vector2(14, 14);
            thumb.ParticipatesInLayout = false;

            Add(track);
            Add(thumb);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            float width = Rect.Size.X;
            float height = Rect.Size.Y;

            float trackHeight = height * TrackThicknessRatio;
            float thumbSize = height * ThumbSizeRatio;

            // center track
            Vector2 trackSize = new(width, trackHeight);
            Vector2 trackPos = new(
                Rect.Position.X,
                Rect.Position.Y + (height - trackHeight) * 0.5f
            );

            track.Arrange(new UITransform(trackPos, trackSize));

            thumb.Size = new Vector2(thumbSize, thumbSize);
            thumbSize = Math.Max(thumbSize, MinThumbSize);

            UpdateThumb();
        }

        private void UpdateThumb()
        {

            float t = (Value - Min) / (Max - Min);
            t = Math.Clamp(t, 0f, 1f);

            Vector2 thumbSize = thumb.Size.GetValueOrDefault();

            float x = Rect.Position.X + t * (Rect.Size.X - thumbSize.X);

            Vector2 pos = new(
                x,
                Rect.Position.Y + (Rect.Size.Y - thumbSize.X) * 0.5f
            );

            thumb.Arrange(new UITransform(pos, thumbSize));
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {
            
        }

        float Snap(float value)
        {
            if (Step <= 0f)
                return value;

            return MathF.Round(value / Step) * Step;
        }

        float MouseToValue(Vector2 mouse)
        {

            float t = (mouse.X - Rect.Position.X) / Rect.Size.X;
            t = Math.Clamp(t, 0f, 1f);

            float value = Min + t * (Max - Min);

            return Snap(value);
        }

        public override void OnMouseDown()
        {
            base.OnMouseDown();

            Value = MouseToValue(UISystem.mousePosition);
        }

        //public override void OnDragStart(Vector2 mouse)
        //{
        //    Value = MouseToValue(mouse);
        //}

        public override void OnDrag(Vector2 mouse, Vector2 delta)
        {
            base.OnDrag(mouse, delta);

            Value = MouseToValue(mouse);
        }
    }
}