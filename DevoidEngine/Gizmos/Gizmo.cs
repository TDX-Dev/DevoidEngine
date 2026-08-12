using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public abstract class Gizmo
    {
        public abstract void Draw(GizmoContext context, GizmoDrawList drawList);

        public virtual bool HitTest(
            GizmoContext context,
            Ray ray,
            out GizmoHit hit)
        {
            hit = default;
            return false;
        }

        public virtual void BeginDrag(
            GizmoContext context,
            GizmoHit hit)
        {
        }

        public virtual void Drag(
            GizmoContext context)
        {
        }

        public virtual void EndDrag(
            GizmoContext context)
        {
        }
    }
}
