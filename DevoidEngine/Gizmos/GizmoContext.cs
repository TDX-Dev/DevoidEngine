using DevoidEngine.Core;
using DevoidEngine.Rendering;
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
        public Viewport Viewport { get; }
        public Camera Camera { get; }

        public Vector2 MousePosition { get; internal set; }
        public Vector2 MouseDelta { get; internal set; }

        public bool MouseDown { get; internal set; }
        public bool MousePressed { get; internal set; }
        public bool MouseReleased { get; internal set; }

        public Gizmo? HoveredGizmo { get; internal set; }
        public GizmoHit? HoveredHit { get; internal set; }

        public Gizmo? ActiveGizmo { get; internal set; }
        public GizmoHit? ActiveHit { get; internal set; }

        public object? Target { get; set; }

        internal GizmoDragState? DragState { get; set; }

        internal GizmoContext(
            Viewport viewport,
            Camera camera)
        {
            Viewport = viewport;
            Camera = camera;
        }
    }
}
