using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class FlexNode : UINode
    {

        public FlexDirection Direction = FlexDirection.Row;
        public JustifyContent Justify = JustifyContent.Start;
        public AlignItems Align = AlignItems.Stretch;
        public float Gap = 0f;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            float main = 0;
            float cross = 0;
            int count = 0;


            foreach (var child in _children)
            {
                if (!child.ParticipatesInLayout)
                    continue;

                var childSize = child.Measure(availableSize);

                float childMain = FlexboxTools.Main(childSize, Direction);
                float childCross = FlexboxTools.Cross(childSize, Direction);

                main += childMain;
                cross = MathF.Max(cross, childCross);
                count++;
            }

            if (count > 1)
                main += Gap * (count - 1);

            return FlexboxTools.FromMainCross(main, cross, Direction);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            float containerMain = FlexboxTools.Main(finalRect.size, Direction);
            float containerCross = FlexboxTools.Cross(finalRect.size, Direction);

            float totalGrow = 0f;
            float fixedMain = 0f;
            int count = 0;

            // ---- PASS 1: classify children ----
            foreach (var child in _children)
            {
                if (!child.ParticipatesInLayout)
                    continue;

                if (child.Layout.FlexGrowMain > 0)
                    totalGrow += child.Layout.FlexGrowMain;
                else
                    fixedMain += FlexboxTools.Main(child.DesiredSize, Direction);

                count++;
            }

            float totalGap = Math.Max(0, Gap * (count - 1));
            float freeForFlex = Math.Max(0, containerMain - fixedMain - totalGap);

            // ---- PASS 2: resolve final sizes ----
            float usedMain = 0f;

            Span<float> resolvedMainSizes = count <= 64
                ? stackalloc float[count]
                : new float[count];

            int index = 0;

            foreach (var child in _children)
            {
                if (!child.ParticipatesInLayout)
                    continue;

                float mainSize;

                if (child.Layout.FlexGrowMain > 0 && totalGrow > 0)
                {
                    // flex-basis: 0  (matches CSS flex: 1 1 0)
                    mainSize = freeForFlex * (child.Layout.FlexGrowMain / totalGrow);
                }
                else
                {
                    mainSize = FlexboxTools.Main(child.DesiredSize, Direction);
                }

                resolvedMainSizes[index++] = mainSize;
                usedMain += mainSize;
            }

            float remainingSpace =
                Math.Max(0, containerMain - usedMain - totalGap);

            // ---- PASS 3: justify-content (POST flex!) ----
            float gap = Gap;
            float cursor = 0f;

            if (Justify != JustifyContent.Start && count > 0)
            {
                switch (Justify)
                {
                    case JustifyContent.Center:
                        cursor = remainingSpace * 0.5f;
                        break;

                    case JustifyContent.End:
                        cursor = remainingSpace;
                        break;

                    case JustifyContent.SpaceBetween:
                        gap = count > 1 ? remainingSpace / (count - 1) : 0;
                        break;

                    case JustifyContent.SpaceAround:
                        gap = remainingSpace / count;
                        cursor = gap * 0.5f;
                        break;

                    case JustifyContent.SpaceEvenly:
                        gap = remainingSpace / (count + 1);
                        cursor = gap;
                        break;
                }
            }

            // ---- PASS 4: place children ----
            index = 0;

            foreach (var child in _children)
            {
                if (!child.ParticipatesInLayout)
                    continue;

                float mainSize = resolvedMainSizes[index++];

                float crossSize = Align == AlignItems.Stretch
                    ? containerCross
                    : FlexboxTools.Cross(child.DesiredSize, Direction);

                float crossOffset = Align switch
                {
                    AlignItems.Start => 0f,
                    AlignItems.Center => (containerCross - crossSize) * 0.5f,
                    AlignItems.End => containerCross - crossSize,
                    AlignItems.Stretch => 0f,
                    _ => 0f
                };

                Vector2 pos = finalRect.position +
                    FlexboxTools.FromMainCross(cursor, crossOffset, Direction);

                Vector2 size =
                    FlexboxTools.FromMainCross(mainSize, crossSize, Direction);

                child.Arrange(new UITransform(pos, size));

                cursor += mainSize + gap;
            }
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel)
        {

        }

        protected override void InitializeCore()
        {

        }
    }
}
