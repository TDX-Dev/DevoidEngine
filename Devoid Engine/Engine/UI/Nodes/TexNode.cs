using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Nodes
{
    class TexNode : FlexboxNode
    {
        public Texture2D texture;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            base.MeasureCore(availableSize);
            return availableSize;
        }

        protected override void ArrangeCore(UITransform finalRect)
        {
            base.ArrangeCore(finalRect);
            //UIRenderer.DrawTexRect(finalRect, texture, DEBUG_NUM_LOCAL);
        }

        //protected override Vector2 MeasureCore(Vector2 available)
        //{
        //    Vector2 desired = Size ?? Vector2.Zero;

        //    foreach (var child in _children)
        //    {
        //        var childDesired = child.Measure(available);
        //        desired = Vector2.Max(desired, childDesired);
        //    }

        //    return desired;
        //}

        //protected override void ArrangeCore(UITransform finalRect)
        //{
        //    Rect = finalRect;

        //    for (int i = 0; i < _children.Count; i++)
        //    {
        //        var child = _children[i];
        //        child.Arrange(new UITransform(
        //            finalRect.position,
        //            child.DesiredSize
        //        ));
        //    }

        //    UIRenderer.DrawRect( finalRect , DEBUG_NUM_LOCAL);
        //}

        protected override void RenderCore(List<RenderItem> renderList, Matrix4x4 canvasModel)
        {
        }
    }
}
