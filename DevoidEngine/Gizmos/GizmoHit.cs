namespace DevoidEngine.Gizmos
{
    public struct GizmoHit
    {
        public Gizmo Gizmo;
        public int Handle;
        public float Distance;
        public GizmoDragConstraint Constraint;

        public GizmoHit(
            Gizmo gizmo,
            int handle,
            float distance,
            GizmoDragConstraint constraint
        )
        {
            Gizmo = gizmo;
            Handle = handle;
            Distance = distance;
            Constraint = constraint;
        }
    }
}