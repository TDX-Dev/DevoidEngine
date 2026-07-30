using DevoidEngine.AssetPipeline;
using DevoidEngine.UI.Text;
using System.Numerics;

namespace DevoidEngine.UI.UINodes
{
    public class ButtonNode : ContainerNode
    {
        private readonly LabelNode label;

        bool hovered;
        bool pressed;

        public Action? OnPressed;
        public override string ThemeType => "Button";

        public string Text
        {
            get => label.Text;
            set => label.Text = value;
        }

        public ButtonNode(string text = "Button")
        {
            BlockInput = true;

            Padding = Padding.GetAll(6);
            Justify = JustifyContent.Center;
            Align = AlignItems.Center;

            Layout.FlexGrowMain = 0;
            Layout.FlexGrowCross = 0;

            Font valveoracle = Asset.Load<Font>("valveoracle-semibold.ttf")!;
            label = new LabelNode()
            {
                FontSize = 16,
                Text = text,
                Font = valveoracle,
                Overflow = TextOverflow.Wrap
            };

            MinSize = new Vector2(35);


            Add(label);


            OnNodeMouseEnter += () =>
            {
                hovered = true;
                UpdateState();
            };

            OnNodeMouseLeave += () =>
            {
                hovered = false;
                pressed = false;
                UpdateState();
            };

            OnNodeMouseDown += () =>
            {
                pressed = true;
                UpdateState();
            };

            OnNodeMouseUp += () =>
            {
                if (pressed)
                    OnPressed?.Invoke();

                pressed = false;
                UpdateState();
            };
        }

        void UpdateState()
        {
            State = UIState.Normal;

            if (hovered)
                State |= UIState.Hover;

            if (pressed)
                State |= UIState.Pressed;

            ApplyTheme();
        }
    }
}
