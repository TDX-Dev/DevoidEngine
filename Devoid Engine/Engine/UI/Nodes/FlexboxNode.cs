using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class FlexboxNode : UINode
    {
        public FlexDirection Direction = FlexDirection.Row;
        public JustifyContent Justify = JustifyContent.Start;
        public AlignItems Align = AlignItems.Stretch;
        public float Gap = 0f;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            float availableMainAxisSize = FlexboxTools.Main(availableSize, Direction);

            float maximumCrossAxisSize = 0f;
            float totalMainAxisSize = 0f;

            int count = 0;

            foreach (var child in _children)
            {
                if (!child.Visible || !child.Interactable)
                    continue;

                Vector2 childAvailableSize = Direction == FlexDirection.Row
                    ? new Vector2(float.PositiveInfinity, availableSize.Y)
                    : new Vector2(availableSize.X, float.PositiveInfinity);

                Vector2 childSize = child.Measure(childAvailableSize);

                float childMainAxisSize = FlexboxTools.Main(childSize, Direction);
                float childCrossAxisSize = FlexboxTools.Cross(childSize, Direction);

                totalMainAxisSize += childMainAxisSize;
                maximumCrossAxisSize = Math.Max(maximumCrossAxisSize, childCrossAxisSize);
                count++;
            }

            if (count > 1)
            {
                totalMainAxisSize += (Gap) * (count - 1);
            }

            return FlexboxTools.FromMainCross(totalMainAxisSize, maximumCrossAxisSize, Direction);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;
            //Console.WriteLine("---- HEADER CONTAINER ----");
            //Console.WriteLine($"finalRect.size: {finalRect.size}");
            //Console.WriteLine($"DesiredSize: {DesiredSize}");


            float containerMain = FlexboxTools.Main(finalRect.size, Direction);
            float containerCross = FlexboxTools.Cross(finalRect.size, Direction);

            List<UINode> children = _children.Where(x => x.Visible && x.ParticipatesInLayout).ToList();

            float totalGap = Math.Max(0, Gap * (children.Count - 1));
            float remainingSpace = containerMain;

            // from my limited newly acquired knowledge of stackalloc, lets give it a go!
            Span<float> resolvedMainSizes = children.Count <= 64 ? stackalloc float[children.Count] : new float[children.Count];
            Span<bool> frozenItems = children.Count <= 64 ? stackalloc bool[children.Count] : new bool[children.Count];

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                float intrinsic = Math.Clamp(
                    FlexboxTools.Main(child.DesiredSize, Direction),
                    FlexboxTools.Main(child.MinSize, Direction),
                    FlexboxTools.Main(child.MaxSize, Direction)
                );

                resolvedMainSizes[i] = intrinsic;
                remainingSpace -= intrinsic;

                if (child.Layout.FlexGrowMain <= 0f)
                {
                    frozenItems[i] = true;
                }

            }

            //if (resolvedMainSizes.Length > 0)
            //{
            //    Console.WriteLine($"child resolvedMainSize: {resolvedMainSizes[0]}");
            //}

            remainingSpace -= totalGap;


            // here we compute the sizes for each child based on its flexGrow and redistribute
            while (remainingSpace > 0f)
            {
                float totalGrow = 0f;

                for (int i = 0; i < children.Count; i++)
                {
                    if (!frozenItems[i])
                    {
                        totalGrow += children[i].Layout.FlexGrowMain;
                    }
                }

                if (totalGrow <= 0f)
                    break;

                bool anyFrozenThisPass = false;

                for (int i = 0; i < children.Count; i++)
                {
                    if (frozenItems[i]) { continue; }

                    var child = children[i];


                    float minSize = FlexboxTools.Main(child.MinSize, Direction);
                    float maxSize = FlexboxTools.Main(child.MaxSize, Direction);


                    float growDelta = remainingSpace * (child.Layout.FlexGrowMain / totalGrow);

                    float proposed = resolvedMainSizes[i] + growDelta;

                    float clamped = Math.Clamp(proposed, minSize, maxSize);

                    if (Math.Abs(proposed - clamped) > 0.0001f)
                    {
                        remainingSpace -= (clamped - resolvedMainSizes[i]);
                        resolvedMainSizes[i] = clamped;
                        frozenItems[i] = true;
                        anyFrozenThisPass = true;
                    }

                }


                if (!anyFrozenThisPass) break;
            }

            float finalGrow = 0f;
            for (int i = 0; i < children.Count; i++)
            {
                if (!frozenItems[i])
                {
                    finalGrow += children[i].Layout.FlexGrowMain;
                }
            }

            if (finalGrow > 0f && remainingSpace > 0f)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (frozenItems[i])
                        continue;

                    float growDelta = remainingSpace * (children[i].Layout.FlexGrowMain / finalGrow);

                    resolvedMainSizes[i] += growDelta;
                }
            }


            float usedMain = 0f;
            for (int i = 0; i < children.Count; i++)
            {
                usedMain += resolvedMainSizes[i];
            }
            float freeSpace = Math.Max(0f, containerMain - usedMain - totalGap);
            float cursor = 0f;
            float gap = Gap;

            FlexboxTools.ComputeJustify(
                Justify,
                freeSpace,
                children.Count,
                Gap,
                out cursor,
                out gap
            );

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                float mainSize = resolvedMainSizes[i];
                float crossSize = Align == AlignItems.Stretch ? containerCross : FlexboxTools.Cross(child.DesiredSize, Direction);

                float crossOffset = FlexboxTools.ComputeCrossOffset(Align, containerCross, crossSize);

                Vector2 pos = finalRect.position + FlexboxTools.FromMainCross(cursor, crossOffset, Direction);
                Vector2 size = FlexboxTools.FromMainCross(mainSize, crossSize, Direction);

                child.Arrange(new UITransform(pos, size));
                cursor += mainSize + gap;
            }

            for (int i = 0; i < _children.Count; i++)
            {
                if (!_children[i].ParticipatesInLayout)
                {
                    var child = _children[i];
                    Vector2 size = child.Size ?? child.DesiredSize;

                    Vector2 pos = finalRect.position + child.Offset;

                    child.Arrange(new UITransform(pos, size));
                }
            }

            //UIRenderer.DrawRect(finalRect, DEBUG_NUM_LOCAL);
        }
        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel)
        {

        }

        protected override void InitializeCore()
        {

        }
    }
}
