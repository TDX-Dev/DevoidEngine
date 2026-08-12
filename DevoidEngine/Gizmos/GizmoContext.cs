using DevoidEngine.Core;
using DevoidEngine.Rendering;
using System.Numerics;

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

        internal GizmoDrawList DrawList { get; } = new();

        internal GizmoContext(
            Viewport viewport,
            Camera camera)
        {
            Viewport = viewport;
            Camera = camera;
        }

        internal void ResetFrame()
        {
            MousePressed = false;
            MouseReleased = false;
            MouseDelta = Vector2.Zero;

            DrawList.Clear();
        }

        internal void EndFrame()
        {
        }
    }
}