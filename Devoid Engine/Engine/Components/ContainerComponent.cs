using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class ContainerComponent : UIComponent
    {
        public override string Type => nameof(ContainerComponent);

        public JustifyContent Justify
        {
            get => justify;
            set
            {
                justify = value;
                if (container != null)
                    container.Justify = justify;
            }
        }

        public AlignItems Align
        {
            get => alignItems;
            set
            {
                alignItems = value;
                if (container != null)
                    container.Align = alignItems;
            }
        }




        private ContainerNode container;
        private JustifyContent justify = JustifyContent.Start;
        private AlignItems alignItems = AlignItems.Start;
        private Padding padding = Padding.GetAll(0);
        private Vector4 color = Vector4.Zero;
        private Vector4 borderColor = Vector4.Zero;
        private float borderThickness = 0;

        protected override UINode BuildNode()
        {
            container = new ContainerNode()
            {
                ParticipatesInLayout = false,
                Offset = new Vector2(50, 350),
                Direction = FlexDirection.Column,
                Gap = 10,
            };

            // apply stored values AFTER creation
            //container.Color = color;
            container.Justify = justify;
            container.Align = alignItems;
            container.Padding = padding;

            return container;
        }
    }
}