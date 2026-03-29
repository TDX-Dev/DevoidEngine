using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    public enum CanvasRenderMode
    {
        ScreenSpace,
        WorldSpace
    }

    public class CanvasNode : FlexboxNode
    {
        public CanvasRenderMode RenderMode;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            base.MeasureCore(availableSize);
            return availableSize;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            base.ArrangeCore(finalRect);
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {

        }
    }
}