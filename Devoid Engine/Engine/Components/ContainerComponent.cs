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
                if (IsInitialized)
                    container.Justify = justify;
            }
        }

        public AlignItems Align
        {
            get => alignItems;
            set
            {
                alignItems = value;
                if (IsInitialized)
                    container.Align = alignItems;
            }
        }

        public Padding Padding
        {
            get => padding;
            set
            {
                padding = value;
                if (IsInitialized)
                    container.Padding = padding;
            }
        }

        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                if (IsInitialized)
                    container.Color = color;
            }
        }

        private ContainerNode container;
        private JustifyContent justify = JustifyContent.Start;
        private AlignItems alignItems = AlignItems.Start;
        private Padding padding = Padding.GetAll(0);
        private Vector4 color = Vector4.Zero;

        protected override UINode BuildNode()
        {
            container = new ContainerNode()
            {
                ParticipatesInLayout = false,
                Offset = new Vector2(50, 250),
                Direction = FlexDirection.Column,
                Color = color,
                Justify = justify,
                Align = alignItems,
                Padding = padding,
                Gap = 10,
                Layout =
                {
                    FlexGrowMain = 0
                }
            };

            return container;
        }
    }
}