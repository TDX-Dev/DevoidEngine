using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public abstract class GizmoDragConstraint
    {
        public abstract bool TryGetPosition(
            GizmoContext context,
            Vector2 mousePosition,
            out Vector3 position);
    }
}
