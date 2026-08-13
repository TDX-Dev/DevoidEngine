using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public abstract class Gizmo
    {
        public bool Enabled { get; set; } = true;

        public abstract void Draw(GizmoContext context);
        public abstract bool HitTest(GizmoContext context, Vector2 mousePosition, out GizmoHit hit);
        public virtual void OnBeginDrag(GizmoContext context, GizmoHit hit) { }
        public virtual void OnDrag(GizmoContext context, GizmoHit hit) { }
        public virtual void OnEndDrag(GizmoContext context, GizmoHit hit) { }
    }
}
