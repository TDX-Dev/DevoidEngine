using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class ButtonComponent : UIComponent
    {
        public override string Type => nameof(ButtonComponent);

        public string Text
        {
            get => text;
            set
            {
                text = value;

                if (label != null)
                    label.Text = value;
            }
        }

        public event Action OnClick;

        private bool isHovering;

        private ContainerNode container;
        private LabelNode label;
        private string text = "Button";
        private float btnScale = 3;
        private float buttonBorderThickness = 0;

        protected override UINode BuildNode()
        {
            container = new ContainerNode()
            {
                Size = new Vector2(200, 40) * btnScale,
                BlockInput = true,
                Justify = JustifyContent.Start,
                Align = AlignItems.Center,
                Padding = Padding.GetAll(5 * btnScale),
                //BorderRadius = new Vector4(10) * btnScale,
                Layout =
                {
                    //FlexGrowMain = 1
                }
            };

            var font = FontLibrary.LoadFont(
                "Engine/Content/Fonts/JetBrainsMono-Regular.ttf",
                32
            );

            label = new LabelNode(text, font, 20f * btnScale);

            container.Add(label);


            // pressed
            container.OnNodeMouseDown = () =>
            {
                OnClick?.Invoke();
            };

            // released
            container.OnNodeMouseUp = () =>
            {

            };

            container.OnNodeMouseHeld = () =>
            {
            };

            container.OnNodeMouseEnter = () =>
            {

            };

            container.OnNodeMouseLeave = () =>
            {

            };

            return container;
        }
    }
}