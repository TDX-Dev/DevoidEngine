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
        public Matrix4x4 WorldMatrix;
        public Vector3 WorldPosition;
        public Vector3 WorldForward;
        public int PixelsPerUnit = 10;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            return base.MeasureCore(availableSize);
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            base.ArrangeCore(finalRect);
        }

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel, int order)
        {

        }

        public Matrix4x4 GetWorldCanvasMatrix()
        {
            Matrix4x4 flipY = Matrix4x4.CreateScale(1f, -1f, 1f);
            Matrix4x4 scale = Matrix4x4.CreateScale(1f / PixelsPerUnit);

            Vector2 canvasSize = DesiredSize;

            Matrix4x4 centerOffset =
                Matrix4x4.CreateTranslation(
                    -canvasSize.X * 0.5f,
                    -canvasSize.Y * 0.5f,
                    0f);


            return centerOffset *
                   flipY *
                   scale *
                   WorldMatrix;
        }
    }
}