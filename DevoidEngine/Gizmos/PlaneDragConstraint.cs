using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public sealed class PlaneDragConstraint : GizmoDragConstraint
    {
        private readonly Vector3 origin;
        private readonly Vector3 normal;

        public PlaneDragConstraint(
            Vector3 origin,
            Vector3 normal)
        {
            this.origin = origin;
            this.normal = Vector3.Normalize(normal);
        }

        public override bool TryGetPosition(
            GizmoContext context,
            Vector2 mousePosition,
            out Vector3 position)
        {
            Ray ray = context.GetMouseRay(mousePosition);

            float denominator =
                Vector3.Dot(
                    ray.Direction,
                    normal);

            if (MathF.Abs(denominator) < 0.00001f)
            {
                position = default;
                return false;
            }

            float t =
                Vector3.Dot(
                    origin - ray.Origin,
                    normal) /
                denominator;

            if (t < 0)
            {
                position = default;
                return false;
            }

            position =
                ray.Origin +
                ray.Direction * t;

            return true;
        }
    }
}