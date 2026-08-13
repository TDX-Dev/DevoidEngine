using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public sealed class BoundsGizmo : Gizmo
    {
        private Vector3 min = Vector3.Zero;
        private Vector3 max = Vector3.One;

        private Vector3 dragStartMin;
        private Vector3 dragStartMax;
        private Vector3 dragStartMouse;

        private GizmoDragConstraint? dragConstraint;

        private const int MinX = 0;
        private const int MaxX = 1;
        private const int MinY = 2;
        private const int MaxY = 3;
        private const int MinZ = 4;
        private const int MaxZ = 5;

        private const float HandleRadius = 0.5f;
        private const float HitRadius = 8.0f;
        private const float MinimumSize = 0.01f;

        public Vector3 Min => min;
        public Vector3 Max => max;

        public override void Draw(GizmoContext context)
        {
            context.DrawList.AddWireBox(min, max, Vector4.One);

            for (int i = 0; i < 6; i++)
            {
                Vector3 position =
                    GetHandlePosition(i, min, max);

                context.DrawList.AddCircle(
                    position,
                    HandleRadius,
                    Vector3.UnitY, new Vector4(0.5f, 0.5f, 0.5f, 1));
            }
        }

        public override bool HitTest(
            GizmoContext context,
            Vector2 mousePosition,
            out GizmoHit hit)
        {
            GizmoHit? bestHit = null;

            for (int i = 0; i < 6; i++)
            {
                Vector3 position = GetHandlePosition(i, min, max);

                Vector3 screen =
                    context.Camera.GetCamera().WorldToScreen(
                        position,
                        context.Viewport.Width,
                        context.Viewport.Height);

                // WorldToScreen currently puts clip.W in Z.
                if (screen.Z <= 0)
                    continue;

                Vector2 screenPosition =
                    new(screen.X, screen.Y);

                float distance =
                    Vector2.Distance(
                        mousePosition,
                        screenPosition);

                if (distance > HitRadius)
                    continue;

                if (bestHit.HasValue &&
                    distance >= bestHit.Value.Distance)
                {
                    continue;
                }

                Vector3 axis =
                    GetHandleAxis(i);

                Vector3 billboardNormal = -context.Camera.GetCamera().Front;

                bestHit = new GizmoHit
                {
                    Gizmo = this,
                    Handle = i,
                    Distance = distance,

                    Constraint =
                        new AxisDragConstraint(
                            position,
                            axis,
                            billboardNormal
                        )
                };
            }

            if (bestHit.HasValue)
            {
                hit = bestHit.Value;
                return true;
            }

            hit = default;
            return false;
        }

        public override void OnBeginDrag(
            GizmoContext context,
            GizmoHit hit)
        {
            dragStartMin = min;
            dragStartMax = max;

            dragConstraint = hit.Constraint;

            if (dragConstraint == null)
                return;

            if (!dragConstraint.TryGetPosition(
                    context,
                    context.MousePosition,
                    out dragStartMouse))
            {
                dragConstraint = null;
            }
        }

        public override void OnDrag(
            GizmoContext context,
            GizmoHit hit)
        {
            if (dragConstraint == null)
                return;

            if (!dragConstraint.TryGetPosition(
                    context,
                    context.MousePosition,
                    out Vector3 mousePosition))
            {
                return;
            }

            Vector3 delta =
                mousePosition - dragStartMouse;

            ApplyDelta(hit.Handle, delta);
        }

        public override void OnEndDrag(
            GizmoContext context,
            GizmoHit hit)
        {
            dragConstraint = null;
        }

        private void ApplyDelta(
            int handle,
            Vector3 delta)
        {
            switch (handle)
            {
                case MinX:
                    {
                        float value =
                            dragStartMin.X + delta.X;

                        min.X =
                            MathF.Min(
                                value,
                                dragStartMax.X - MinimumSize);

                        break;
                    }

                case MaxX:
                    {
                        float value =
                            dragStartMax.X + delta.X;

                        max.X =
                            MathF.Max(
                                value,
                                dragStartMin.X + MinimumSize);

                        break;
                    }

                case MinY:
                    {
                        float value =
                            dragStartMin.Y + delta.Y;

                        min.Y =
                            MathF.Min(
                                value,
                                dragStartMax.Y - MinimumSize);

                        break;
                    }

                case MaxY:
                    {
                        float value =
                            dragStartMax.Y + delta.Y;

                        max.Y =
                            MathF.Max(
                                value,
                                dragStartMin.Y + MinimumSize);

                        break;
                    }

                case MinZ:
                    {
                        float value =
                            dragStartMin.Z + delta.Z;

                        min.Z =
                            MathF.Min(
                                value,
                                dragStartMax.Z - MinimumSize);

                        break;
                    }

                case MaxZ:
                    {
                        float value =
                            dragStartMax.Z + delta.Z;

                        max.Z =
                            MathF.Max(
                                value,
                                dragStartMin.Z + MinimumSize);

                        break;
                    }
            }
        }

        private static Vector3 GetHandlePosition(
            int handle,
            Vector3 min,
            Vector3 max)
        {
            Vector3 center =
                (min + max) * 0.5f;

            return handle switch
            {
                MinX => new Vector3(
                    min.X,
                    center.Y,
                    center.Z),

                MaxX => new Vector3(
                    max.X,
                    center.Y,
                    center.Z),

                MinY => new Vector3(
                    center.X,
                    min.Y,
                    center.Z),

                MaxY => new Vector3(
                    center.X,
                    max.Y,
                    center.Z),

                MinZ => new Vector3(
                    center.X,
                    center.Y,
                    min.Z),

                MaxZ => new Vector3(
                    center.X,
                    center.Y,
                    max.Z),

                _ => center
            };
        }

        private static Vector3 GetHandleAxis(
            int handle)
        {
            return handle switch
            {
                MinX or MaxX => Vector3.UnitX,
                MinY or MaxY => Vector3.UnitY,
                MinZ or MaxZ => Vector3.UnitZ,

                _ => Vector3.UnitX
            };
        }
    }
}
