using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    using global::DevoidEngine.Core;
    using System.Numerics;

    namespace DevoidEngine.Gizmos
    {
        public sealed class TranslateGizmo : Gizmo
        {
            private GizmoMaterial XMaterial = GizmoMaterial.Colored(new Vector4(1, 0, 0, 1));

            private GizmoMaterial YMaterial = GizmoMaterial.Colored(new Vector4(0, 1, 0, 1));

            private GizmoMaterial ZMaterial = GizmoMaterial.Colored(new Vector4(0, 0, 1, 1));

            private const int XAxis = 0;
            private const int YAxis = 1;
            private const int ZAxis = 2;

            private const float AxisLength = 2.0f;
            private const float AxisThickness = 0.08f;
            private const float HitRadius = 10.0f;

            private Vector3 position;

            private Vector3 dragStartPosition;
            private Vector3 dragStartMouse;

            private GizmoDragConstraint? dragConstraint;

            public Vector3 Position
            {
                get => position;
                set => position = value;
            }

            public override void Draw(GizmoContext context)
            {
                // X
                {
                    Vector3 axis = Vector3.UnitX;

                    Vector3 center =
                        position + axis * (AxisLength * 0.5f);

                    Vector3 halfSize =
                        new(
                            AxisLength * 0.5f,
                            AxisThickness,
                            AxisThickness);

                    context.DrawList.AddBox(
                        center - halfSize,
                        center + halfSize, XMaterial);
                }

                // Y
                {
                    Vector3 axis = Vector3.UnitY;

                    Vector3 center =
                        position + axis * (AxisLength * 0.5f);

                    Vector3 halfSize =
                        new(
                            AxisThickness,
                            AxisLength * 0.5f,
                            AxisThickness);

                    context.DrawList.AddBox(
                        center - halfSize,
                        center + halfSize, YMaterial);
                }

                // Z
                {
                    Vector3 axis = Vector3.UnitZ;

                    Vector3 center =
                        position + axis * (AxisLength * 0.5f);

                    Vector3 halfSize =
                        new(
                            AxisThickness,
                            AxisThickness,
                            AxisLength * 0.5f);

                    context.DrawList.AddBox(
                        center - halfSize,
                        center + halfSize, ZMaterial);
                }
            }

            public override bool HitTest(
                GizmoContext context,
                Vector2 mousePosition,
                out GizmoHit hit)
            {
                GizmoHit? bestHit = null;

                Camera camera = context.Camera.GetCamera();

                for (int axisIndex = 0; axisIndex < 3; axisIndex++)
                {
                    Vector3 axis = GetAxis(axisIndex);

                    Vector3 axisStart = position;
                    Vector3 axisEnd = position + axis * AxisLength;

                    Vector3 startScreen = camera.WorldToScreen(
                        axisStart,
                        context.Viewport.Width,
                        context.Viewport.Height);

                    Vector3 endScreen = camera.WorldToScreen(
                        axisEnd,
                        context.Viewport.Width,
                        context.Viewport.Height);

                    if (startScreen.Z <= 0 || endScreen.Z <= 0)
                        continue;

                    Vector2 a = new(startScreen.X, startScreen.Y);
                    Vector2 b = new(endScreen.X, endScreen.Y);

                    float distance = DistanceToLineSegment(
                        mousePosition,
                        a,
                        b);

                    if (distance > HitRadius)
                        continue;

                    if (bestHit.HasValue &&
                        distance >= bestHit.Value.Distance)
                    {
                        continue;
                    }

                    Vector3 planeNormal =
                        -camera.Front;

                    bestHit = new GizmoHit
                    {
                        Gizmo = this,
                        Handle = axisIndex,
                        Distance = distance,

                        Constraint =
                            new AxisDragConstraint(
                                position,
                                axis,
                                planeNormal)
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
                dragStartPosition = position;
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

                position =
                    dragStartPosition + delta;
            }

            public override void OnEndDrag(
                GizmoContext context,
                GizmoHit hit)
            {
                dragConstraint = null;
            }

            private static Vector3 GetAxis(int axis)
            {
                return axis switch
                {
                    XAxis => Vector3.UnitX,
                    YAxis => Vector3.UnitY,
                    ZAxis => Vector3.UnitZ,
                    _ => Vector3.UnitX
                };
            }

            private static float DistanceToLineSegment(
                Vector2 point,
                Vector2 a,
                Vector2 b)
            {
                Vector2 ab = b - a;

                float lengthSquared = ab.LengthSquared();

                if (lengthSquared < 0.000001f)
                    return Vector2.Distance(point, a);

                float t =
                    Vector2.Dot(point - a, ab) /
                    lengthSquared;

                t = Math.Clamp(t, 0.0f, 1.0f);

                Vector2 closest =
                    a + ab * t;

                return Vector2.Distance(point, closest);
            }
        }
    }
}
