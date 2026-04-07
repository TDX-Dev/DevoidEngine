using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class SplitterNode : ContainerNode
    {
        public UINode? Target; // panel being resized

        public bool Vertical; // true = column layout

        public float Min = 50;
        public float Max = 1000;

        float startBasis;
        Vector2 startMouse;
        bool isDragging;
        bool isHovering;

        public SplitterNode()
        {
            BlockInput = true;
        }

        protected override void InitializeCore()
        {

            base.InitializeCore();
        }

        public override void OnDragStart(Vector2 mouse)
        {
            if (Target == null)
                return;
            startMouse = mouse;

            startBasis = Vertical
                ? Target.Rect.Size.Y
                : Target.Rect.Size.X;

            Target.Layout.FlexBasis = startBasis;
            Target.Layout.FlexGrowMain = 0;

            isDragging = true;
            Cursor.SetCursorShape(
                Vertical ? CursorShape.ResizeNS : CursorShape.ResizeEW
            );
        }

        public override void OnDrag(Vector2 mouse, Vector2 delta)
        {
            if (Target == null) return;
            float d = Vertical ? delta.Y : delta.X;

            float basis = Math.Clamp(Target.Layout.FlexBasis + d, Min, Max);

            Target.Layout.FlexBasis = basis;
        }

        public override void OnDragEnd(Vector2 mouse)
        {
            isDragging = false;
            if (!isHovering)
                Cursor.SetCursorShape(CursorShape.Arrow);
        }

        public override void OnMouseEnter()
        {
            isHovering = true;
            Cursor.SetCursorShape(
                Vertical ? CursorShape.ResizeNS : CursorShape.ResizeEW
            );
        }

        public override void OnMouseLeave()
        {
            isHovering = false;
            if (!isDragging)
                Cursor.SetCursorShape(CursorShape.Arrow);
        }

        public override void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;
                Cursor.SetCursorShape(CursorShape.Arrow);
            }

            base.OnMouseUp();
        }
    }
}
