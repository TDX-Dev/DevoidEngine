using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class FlexboxNode : UINode
    {
        public FlexDirection Direction = FlexDirection.Row;
        public JustifyContent Justify = JustifyContent.Start;
        public AlignItems Align = AlignItems.Stretch;
        public Padding Padding;
        public float Gap = 0f;

        private MaterialInstance debugMaterial;

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

                Vector2 childAvailableSize = new Vector2(
                    Math.Max(0, availableSize.X - Padding.Horizontal),
                    Math.Max(0, availableSize.Y - Padding.Vertical)
                );

                Vector2 childSize = child.Measure(childAvailableSize);

                float childMainAxisSize = FlexboxTools.Main(childSize, Direction);
                float childCrossAxisSize = FlexboxTools.Cross(childSize, Direction);

                totalMainAxisSize += childMainAxisSize;
                maximumCrossAxisSize = Math.Max(maximumCrossAxisSize, childCrossAxisSize);
                count++;
            }

            if (count > 1)
                totalMainAxisSize += Gap * (count - 1);

            Vector2 contentSize = FlexboxTools.FromMainCross(totalMainAxisSize, maximumCrossAxisSize, Direction);

            return new Vector2(
                contentSize.X + Padding.Left + Padding.Right,
                contentSize.Y + Padding.Top + Padding.Bottom
            );

            //Vector2 desired = new Vector2(
            //    contentSize.X + Padding.Left + Padding.Right,
            //    contentSize.Y + Padding.Top + Padding.Bottom
            //);

            //desired.X = Math.Min(desired.X, availableSize.X);
            //desired.Y = Math.Min(desired.Y, availableSize.Y);

            //return desired;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            Rect = finalRect;

            Vector2 contentPos = new Vector2(
                finalRect.position.X + Padding.Left,
                finalRect.position.Y + Padding.Top
            );

            Vector2 contentSize = new Vector2(
                Math.Max(0, finalRect.size.X - Padding.Left - Padding.Right),
                Math.Max(0, finalRect.size.Y - Padding.Top - Padding.Bottom)
            );

            float containerMain = FlexboxTools.Main(contentSize, Direction);
            float containerCross = FlexboxTools.Cross(contentSize, Direction);

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
                    Math.Min(
                        FlexboxTools.Main(child.MaxSize, Direction),
                        containerMain
                    )
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
                float crossSize = Align == AlignItems.Stretch
                    ? containerCross
                    : Math.Min(
                        FlexboxTools.Cross(child.DesiredSize, Direction),
                        containerCross
                    );

                float crossOffset = FlexboxTools.ComputeCrossOffset(Align, containerCross, crossSize);

                Vector2 pos = contentPos + FlexboxTools.FromMainCross(cursor, crossOffset, Direction);
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

                    Vector2 pos = contentPos + child.Offset;

                    child.Arrange(new UITransform(pos, size));
                }
            }
        }
        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {
            Vector2 size = Rect.size;
            Vector2 pos = Rect.position;

            Vector2 pivotOffset = (Pivot - new Vector2(0.5f)) * size;

            Vector2 centerPos = pos + size * 0.5f;

            Matrix4x4 model =
                Matrix4x4.CreateScale(size.X, size.Y, 1f) *
                Matrix4x4.CreateTranslation(pivotOffset.X, pivotOffset.Y, 0f) *
                Matrix4x4.CreateRotationZ(Rotation) *
                Matrix4x4.CreateTranslation(centerPos.X, centerPos.Y, 0f);

            Matrix4x4 final =
                model *
                canvasModel *
                Matrix4x4.CreateTranslation(0, 0, order * UISystem.OrderEpsilon);

            debugMaterial.SetVector2("RECT_SIZE", size);

            DebugRenderSystem.DrawRectUI(model, debugMaterial);
        }

        protected override void InitializeCore()
        {
            debugMaterial = UISystem.DebugMaterial;
        }

        protected override void UpdateCore(float deltaTime)
        {

        }
    }
}