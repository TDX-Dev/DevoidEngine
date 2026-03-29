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

        public Padding Padding
        {
            get => padding;
            set
            {
                padding = value;
                if (container != null)
                    container.Padding = padding;
            }
        }

        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                if (container != null)
                    container.Color = color;
            }
        }

        public float BorderThickness
        {
            get => borderThickness;
            set
            {
                borderThickness = value;
                if (container != null)
                {
                    container.BorderThickness = borderThickness;
                }
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
                Color = Vector4.One,
                Gap = 10,
            };

            // apply stored values AFTER creation
            //container.Color = color;
            container.Justify = justify;
            container.Align = alignItems;
            container.Padding = padding;
            container.BorderThickness = borderThickness;

            return container;
        }
    }
}