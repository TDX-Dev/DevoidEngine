using System.Numerics;
using DevoidEngine.Util;

namespace DevoidEngine.Gizmos
{
    public sealed class AxisDragConstraint : GizmoDragConstraint
    {
        private readonly Vector3 origin;
        private readonly Vector3 axis;
        private readonly Vector3 planeNormal;

        public AxisDragConstraint(
            Vector3 origin,
            Vector3 axis,
            Vector3 planeNormal)
        {
            this.origin = origin;
            this.axis = Vector3.Normalize(axis);
            this.planeNormal = Vector3.Normalize(planeNormal);
        }

        public override bool TryGetPosition(
            GizmoContext context,
            Vector2 mousePosition,
            out Vector3 position)
        {
            Ray ray =
                context.GetMouseRay(mousePosition);


            float denominator =
                Vector3.Dot(
                    ray.Direction,
                    planeNormal);

            if (MathF.Abs(denominator) < 0.00001f)
            {
                position = default;
                return false;
            }

            float t =
                Vector3.Dot(
                    origin - ray.Origin,
                    planeNormal) /
                denominator;

            if (t < 0.0f)
            {
                position = default;
                return false;
            }

            Vector3 planePosition =
                ray.Origin +
                ray.Direction * t;

            float axisDistance =
                Vector3.Dot(
                    planePosition - origin,
                    axis);

            position =
                origin +
                axis * axisDistance;

            return true;
        }
    }
}