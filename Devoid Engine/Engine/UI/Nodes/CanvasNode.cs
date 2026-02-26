using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class CanvasNode : FlexboxNode
    {
        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            base.MeasureCore(availableSize);
            return availableSize;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            base.ArrangeCore(finalRect);
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel)
        {

        }
    }
}
