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
                Size = new Vector2(200, 200),
                BlockInput = true,
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

            return new BoxNode()
            {
                Size = new Vector2(500, 500),
                Color = new Vector4(1,1,1,1),
                BlockInput = true,
            };
        }
    }
}