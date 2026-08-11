using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.UI.UINodes
{
    public class FlexboxNode : UINode
    {
        public FlexDirection Direction = FlexDirection.Row;
        public JustifyContent Justify = JustifyContent.Start;
        public AlignItems Align = AlignItems.Stretch;
        public FlexWrap Wrap = FlexWrap.NoWrap;
        public Padding Padding = new();
        public float Gap = 0f;


        public FlexboxNode()
        {

        }

        protected override void InitializeCore()
        {

        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            Vector2 innerAvailableSize = new(
                Math.Max(0, availableSize.X - Padding.Horizontal),
                Math.Max(0, availableSize.Y - Padding.Vertical)
            );

            float availableMainAxisSize = FlexboxTools.Main(innerAvailableSize, Direction);
            bool constrainedMain = availableMainAxisSize > 0f;

            float maxLineMainSize = 0f;
            float totalCrossSize = 0f;

            float currentLineMainSize = 0f;
            float currentLineCrossSize = 0f;
            int currentLineCount = 0;

            var children = GetLayoutChildren();

            foreach (var child in children)
            {
                //if (!child.Visible || !child.ParticipatesInLayout)
                //    continue;

                Vector2 childSize = child.Measure(innerAvailableSize);

                float childMainMin = FlexboxTools.Main(child.MinSize, Direction);
                float childMainMax = FlexboxTools.Main(child.MaxSize, Direction);
                float childCrossSize = FlexboxTools.Cross(childSize, Direction);

                float basis = child.Layout.FlexBasis > 0f
                    ? child.Layout.FlexBasis
                    : FlexboxTools.Main(childSize, Direction);

                float childMainAxisSize = Math.Clamp(basis, childMainMin, childMainMax);

                float needed = childMainAxisSize + (currentLineCount > 0 ? Gap : 0f);

                if (Wrap == FlexWrap.Wrap &&
                    constrainedMain &&
                    currentLineCount > 0 &&
                    currentLineMainSize + needed > availableMainAxisSize)
                {
                    maxLineMainSize = Math.Max(maxLineMainSize, currentLineMainSize);
                    totalCrossSize += currentLineCrossSize;
                    if (totalCrossSize > 0f)
                        totalCrossSize += Gap;

                    currentLineMainSize = 0f;
                    currentLineCrossSize = 0f;
                    currentLineCount = 0;
                }

                if (currentLineCount > 0)
                    currentLineMainSize += Gap;

                currentLineMainSize += childMainAxisSize;
                currentLineCrossSize = Math.Max(currentLineCrossSize, childCrossSize);
                currentLineCount++;
            }

            if (currentLineCount > 0)
            {
                maxLineMainSize = Math.Max(maxLineMainSize, currentLineMainSize);
                totalCrossSize += currentLineCrossSize;
            }

            float contentMainSize = Wrap == FlexWrap.Wrap ? maxLineMainSize : currentLineMainSize;
            float contentCrossSize = Wrap == FlexWrap.Wrap ? totalCrossSize : currentLineCrossSize;

            Vector2 contentSize = FlexboxTools.FromMainCross(contentMainSize, contentCrossSize, Direction);

            return new Vector2(
                contentSize.X + Padding.Horizontal,
                contentSize.Y + Padding.Vertical
            );
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            if (Wrap == FlexWrap.NoWrap)
                ArrangeSingleLine(finalRect);
            else
                ArrangeWrapped(finalRect);
        }

        void ArrangeWrapped(Rect finalRect)
        {
            Rect = finalRect;

            Vector2 contentPos = new(
                finalRect.Position.X + Padding.Left,
                finalRect.Position.Y + Padding.Top
            );

            Vector2 contentSize = new(
                Math.Max(0, finalRect.Size.X - Padding.Left - Padding.Right),
                Math.Max(0, finalRect.Size.Y - Padding.Top - Padding.Bottom)
            );

            float containerMain = FlexboxTools.Main(contentSize, Direction);
            float containerCross = FlexboxTools.Cross(contentSize, Direction);

            List<UINode> children = GetLayoutChildren();
            int count = children.Count;

            Vector2 childAvailable = FlexboxTools.FromMainCross(containerMain, containerCross, Direction);

            for (int i = 0; i < count; i++)
                children[i].Measure(childAvailable);

            Span<float> resolvedMainSizes = count <= 64 ? stackalloc float[count] : new float[count];
            Span<float> resolvedCrossSizes = count <= 64 ? stackalloc float[count] : new float[count];

            for (int i = 0; i < count; i++)
            {
                var child = children[i];

                float basis = FlexboxTools.IsFitContent(Direction, child, true)
                    ? FlexboxTools.Main(child.DesiredSize, Direction)
                    : child.Layout.FlexBasis > 0f
                        ? child.Layout.FlexBasis
                        : FlexboxTools.Main(child.DesiredSize, Direction);

                resolvedMainSizes[i] = Math.Clamp(
                    basis,
                    FlexboxTools.Main(child.MinSize, Direction),
                    FlexboxTools.Main(child.MaxSize, Direction)
                );

                resolvedCrossSizes[i] = Math.Clamp(
                    FlexboxTools.Cross(child.DesiredSize, Direction),
                    FlexboxTools.Cross(child.MinSize, Direction),
                    FlexboxTools.Cross(child.MaxSize, Direction)
                );
            }

            List<FlexLine> lines = [];

            float lineMain = 0f;
            float lineCross = 0f;
            int lineStart = 0;
            int lineCount = 0;

            for (int i = 0; i < count; i++)
            {
                float needed = resolvedMainSizes[i] + (lineCount > 0 ? Gap : 0f);

                if (lineCount > 0 && lineMain + needed > containerMain)
                {
                    lines.Add(new FlexLine
                    {
                        startIndex = lineStart,
                        count = lineCount,
                        mainSize = lineMain,
                        crossSize = lineCross
                    });

                    lineStart = i;
                    lineMain = 0f;
                    lineCross = 0f;
                    lineCount = 0;
                }

                if (lineCount > 0)
                    lineMain += Gap;

                lineMain += resolvedMainSizes[i];
                lineCross = Math.Max(lineCross, resolvedCrossSizes[i]);
                lineCount++;
            }

            if (lineCount > 0)
            {
                lines.Add(new FlexLine
                {
                    startIndex = lineStart,
                    count = lineCount,
                    mainSize = lineMain,
                    crossSize = lineCross
                });
            }

            float crossCursor = 0f;

            foreach (var line in lines)
            {
                float cursor = 0f;

                for (int i = 0; i < line.count; i++)
                {
                    var child = children[line.startIndex + i];

                    float mainSize = Math.Clamp(
                        resolvedMainSizes[line.startIndex + i],
                        FlexboxTools.Main(child.MinSize, Direction),
                        FlexboxTools.Main(child.MaxSize, Direction)
                    );

                    float crossSize;

                    if (FlexboxTools.IsFitContent(Direction, child, false))
                    {
                        crossSize = resolvedCrossSizes[line.startIndex + i];
                    }
                    else if (Align == AlignItems.Stretch)
                    {
                        crossSize = line.crossSize;
                    }
                    else
                    {
                        crossSize = Math.Clamp(
                            resolvedCrossSizes[line.startIndex + i],
                            FlexboxTools.Cross(child.MinSize, Direction),
                            line.crossSize
                        );
                    }

                    float crossOffset = FlexboxTools.ComputeCrossOffset(Align, line.crossSize, crossSize);

                    Vector2 pos = contentPos + FlexboxTools.FromMainCross(cursor, crossCursor + crossOffset, Direction);
                    Vector2 size = FlexboxTools.FromMainCross(mainSize, crossSize, Direction);

                    child.Arrange(new Rect(pos, size));

                    cursor += mainSize + Gap;
                }

                crossCursor += line.crossSize + Gap;
            }
        }

        protected void ArrangeSingleLine(Rect finalRect)
        {
            Rect = finalRect;

            Vector2 contentPos = new(
                finalRect.Position.X + Padding.Left,
                finalRect.Position.Y + Padding.Top
            );

            Vector2 contentSize = new(
                Math.Max(0, finalRect.Size.X - Padding.Left - Padding.Right),
                Math.Max(0, finalRect.Size.Y - Padding.Top - Padding.Bottom)
            );

            float containerMain = FlexboxTools.Main(contentSize, Direction);
            float containerCross = FlexboxTools.Cross(contentSize, Direction);

            List<UINode> children = GetLayoutChildren();
            int count = children.Count;

            float totalGap = Math.Max(0, Gap * (count - 1));
            float remainingSpace = containerMain - totalGap;

            Span<float> resolvedMainSizes = count <= 64 ? stackalloc float[count] : new float[count];
            Span<bool> frozenItems = count <= 64 ? stackalloc bool[count] : new bool[count];

            for (int i = 0; i < count; i++)
            {
                var child = children[i];

                //float basis = child.Layout.FlexBasis > 0f
                //    ? child.Layout.FlexBasis
                //    : FlexboxTools.Main(child.DesiredSize, Direction);

                float basis = FlexboxTools.IsFitContent(Direction, child, true)
                    ? FlexboxTools.Main(child.DesiredSize, Direction)
                    : child.Layout.FlexBasis > 0f
                        ? child.Layout.FlexBasis
                        : FlexboxTools.Main(child.DesiredSize, Direction);

                //float minSize = FlexboxTools.Main(child.MinSize, Direction);
                //float maxSize = Math.Min(FlexboxTools.Main(child.MaxSize, Direction), containerMain);

                //float intrinsic = Math.Clamp(basis, minSize, maxSize);

                float minSize = FlexboxTools.Main(child.MinSize, Direction);

                float rawMax = FlexboxTools.Main(child.MaxSize, Direction);
                float maxSize = rawMax > 0f ? rawMax : float.PositiveInfinity;

                float intrinsic = Math.Clamp(basis, minSize, maxSize);

                resolvedMainSizes[i] = intrinsic;
                remainingSpace -= intrinsic;

                if (FlexboxTools.IsFitContent(Direction, child, true) ||
                    child.Layout.FlexGrowMain <= 0f)
                {
                    frozenItems[i] = true;
                }
            }

            while (remainingSpace > 0f)
            {
                float totalGrow = 0f;

                for (int i = 0; i < count; i++)
                {
                    if (!frozenItems[i])
                        totalGrow += children[i].Layout.FlexGrowMain;
                }

                if (totalGrow <= 0f)
                    break;

                bool anyFrozenThisPass = false;

                for (int i = 0; i < count; i++)
                {
                    if (frozenItems[i])
                        continue;

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

                if (!anyFrozenThisPass)
                    break;
            }

            float finalGrow = 0f;
            for (int i = 0; i < count; i++)
            {
                if (!frozenItems[i])
                    finalGrow += children[i].Layout.FlexGrowMain;
            }

            if (finalGrow > 0f && remainingSpace > 0f)
            {
                for (int i = 0; i < count; i++)
                {
                    if (frozenItems[i])
                        continue;

                    float growDelta = remainingSpace * (children[i].Layout.FlexGrowMain / finalGrow);
                    resolvedMainSizes[i] += growDelta;
                }
            }

            if (remainingSpace < 0f)
            {
                Span<bool> shrinkFrozenItems = count <= 64 ? stackalloc bool[count] : new bool[count];

                while (remainingSpace < 0f)
                {
                    float totalShrinkWeight = 0f;

                    for (int i = 0; i < count; i++)
                    {
                        if (!shrinkFrozenItems[i] &&
                            !FlexboxTools.IsFitContent(Direction, children[i], true))
                        {
                            totalShrinkWeight += resolvedMainSizes[i];
                        }
                    }

                    if (totalShrinkWeight <= 0f)
                        break;

                    bool anyFrozenThisPass = false;

                    for (int i = 0; i < count; i++)
                    {
                        if (!shrinkFrozenItems[i] ||
                            !FlexboxTools.IsFitContent(Direction, children[i], true))
                        {
                            continue;
                        }

                        var child = children[i];
                        float minSize = FlexboxTools.Main(child.MinSize, Direction);

                        float shrinkWeight = resolvedMainSizes[i];
                        float shrinkDelta = remainingSpace * (shrinkWeight / totalShrinkWeight);

                        float proposed = resolvedMainSizes[i] + shrinkDelta;
                        float clamped = Math.Max(proposed, minSize);

                        if (Math.Abs(proposed - clamped) > 0.0001f)
                        {
                            remainingSpace -= (clamped - resolvedMainSizes[i]);
                            resolvedMainSizes[i] = clamped;
                            shrinkFrozenItems[i] = true;
                            anyFrozenThisPass = true;
                        }
                    }

                    if (!anyFrozenThisPass)
                        break;
                }

                float finalShrinkWeight = 0f;
                for (int i = 0; i < count; i++)
                {
                    if (!shrinkFrozenItems[i])
                        finalShrinkWeight += resolvedMainSizes[i];
                }

                if (finalShrinkWeight > 0f && remainingSpace < 0f)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (shrinkFrozenItems[i])
                            continue;

                        float shrinkDelta = remainingSpace * (resolvedMainSizes[i] / finalShrinkWeight);
                        resolvedMainSizes[i] += shrinkDelta;
                    }
                }
            }

            float usedMain = 0f;
            for (int i = 0; i < count; i++)
                usedMain += resolvedMainSizes[i];

            float freeSpace = Math.Max(0f, containerMain - usedMain - totalGap);


            FlexboxTools.ComputeJustify(
                Justify,
                freeSpace,
                count,
                Gap,
                out float cursor,
                out float gap
            );

            for (int i = 0; i < count; i++)
            {
                var child = children[i];

                float mainSize = resolvedMainSizes[i];

                float crossSize;

                if (FlexboxTools.IsFitContent(Direction, child, false))
                {
                    crossSize = FlexboxTools.Cross(child.DesiredSize, Direction);
                }
                else if (Align == AlignItems.Stretch || child.Layout.FlexGrowCross > 0f)
                {
                    crossSize = containerCross;
                }
                else
                {
                    crossSize = Math.Min(
                        FlexboxTools.Cross(child.DesiredSize, Direction),
                        containerCross
                    );
                }

                float crossOffset = FlexboxTools.ComputeCrossOffset(Align, containerCross, crossSize);

                Vector2 size = FlexboxTools.FromMainCross(mainSize, crossSize, Direction);
                child.Measure(size);

                Vector2 pos = contentPos + FlexboxTools.FromMainCross(cursor, crossOffset, Direction);
                child.Arrange(new Rect(pos, size));

                cursor += mainSize + gap;
            }

            for (int i = 0; i < _children.Count; i++)
            {
                if (!_children[i].ParticipatesInLayout)
                {
                    var child = _children[i];
                    Vector2 size = child.Size ?? child.DesiredSize;
                    Vector2 pos = contentPos + child.Offset;
                    child.Arrange(new Rect(pos, size));
                }
            }
        }

        protected override void UpdateCore(float deltaTime)
        {
        }

        protected override void PreRender(UIDrawList drawList, int order)
        {

        }

        protected override void PostRender(UIDrawList drawList)
        {

        }
    }
}
