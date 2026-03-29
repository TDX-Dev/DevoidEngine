using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class ButtonComponent : UIComponent
    {
        public override string Type => nameof(ButtonComponent);

        public Vector4 BaseColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
        public Vector4 OnClickColor = new Vector4(0.35f, 0.35f, 0.35f, 1f);
        public Vector4 OnHoverColor = new Vector4(0.4f, 0.4f, 0.4f, 1f);

        public string Text
        {
            get => text;
            set
            {
                text = value;
                if (IsInitialized)
                    label.Text = value;
            }
        }

        public event Action OnClick;

        private bool isHovering;

        private ContainerNode container;
        private LabelNode label;
        private string text = "Button";

        protected override UINode BuildNode()
        {
            container = new ContainerNode()
            {
                Size = new Vector2(200, 40),
                Color = BaseColor,
                BlockInput = true,
                Justify = JustifyContent.Start,
                Align = AlignItems.Center,
                Padding = Padding.GetAll(5),
                Layout =
                {
                    //FlexGrowMain = 1
                }
            };

            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            label = new LabelNode(text, font, 20f)
            {
                //LayoutOptions = new TextLayoutOptions()
                //{
                //    Align = TextAlign.Right,
                //    MaxWidth = 120
                //}
            };

            container.Add(label);

            // pressed
            container.OnNodeMouseDown = () =>
            {
                container.Color = OnClickColor;
                OnClick?.Invoke();
            };

            // released
            container.OnNodeMouseUp = () =>
            {
                if (isHovering)
                    container.Color = OnHoverColor;
                else
                    container.Color = BaseColor;
            };

            container.OnNodeMouseHeld = () =>
            {
                container.Color = OnClickColor;
            };

            container.OnNodeMouseEnter = () =>
            {
                container.Color = OnHoverColor;
                isHovering = true;
            };

            container.OnNodeMouseLeave = () =>
            {
                container.Color = BaseColor;
                isHovering = false;
            };

            return container;
        }
    }
}