using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoContext
    {
        public Viewport Viewport { get; internal set; } = null!;

        public List<Gizmo> Gizmos { get; } = [];

        public GizmoHit? HotHit { get; internal set; }
        public GizmoHit? ActiveHit { get; internal set; }

        public GizmoDrawList DrawList { get; } = new();

        public Camera Camera => Viewport.ActiveCamera!;

        public Vector2 MousePosition { get; internal set; }
        public Vector2 MouseDelta { get; internal set; }

        public bool IsMouseDown { get; internal set; }

        public Ray GetMouseRay(Vector2 mousePosition)
        {
            return Camera.ScreenToWorldRay(
                mousePosition,
                Viewport.Width,
                Viewport.Height);
        }
        public Vector3 GetMousePosition(GizmoDragConstraint constraint)
        {
            if (constraint.TryGetPosition(
                    this,
                    MousePosition,
                    out Vector3 position))
            {
                return position;
            }

            return default;
        }
    }
}
