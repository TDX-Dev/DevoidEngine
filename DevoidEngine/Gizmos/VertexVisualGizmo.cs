using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public class VertexVisualGizmo : Gizmo
    {
        public List<Vector3> Points = [];
        public VertexVisualGizmo(List<Vector3> points)
        {
            Points = points;
        }

        public override void Draw(GizmoContext context)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                context.DrawList.AddCircle(Points[i], 0.5f, Vector3.UnitY, new Vector4(0.5f, 0.5f, 0.5f, 1));
            }
        }

        public override bool HitTest(GizmoContext context, Vector2 mousePosition, out GizmoHit hit)
        {
            throw new NotImplementedException();
        }
    }
}
