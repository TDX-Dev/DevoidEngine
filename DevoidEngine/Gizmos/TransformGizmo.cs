using DevoidEngine.Components;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public sealed class TransformGizmo : Gizmo
    {
        public Transform3D? Target { get; set; }

        public float AxisLength = 1.0f;

        public GizmoStyle XStyle = new()
        {
            Color = new Vector4(1, 0, 0, 1),
            Depth = GizmoDepthMode.Always,
            Outline = GizmoOutlineMode.None
        };

        public GizmoStyle YStyle = new()
        {
            Color = new Vector4(0, 1, 0, 1),
            Depth = GizmoDepthMode.Always,
            Outline = GizmoOutlineMode.None
        };

        public GizmoStyle ZStyle = new()
        {
            Color = new Vector4(0, 0.5f, 1, 1),
            Depth = GizmoDepthMode.Always,
            Outline = GizmoOutlineMode.None
        };

        public override void Draw(
        GizmoContext context,
        GizmoDrawList drawList)
        {
            if (Target == null)
                return;

            Vector3 position = Target.Position;

            drawList.Line(
                position,
                position + Vector3.UnitX,
                XStyle,
                (uint)TransformHandle.X);

            drawList.Line(
                position,
                position + Vector3.UnitY,
                YStyle,
                (uint)TransformHandle.Y);

            drawList.Line(
                position,
                position + Vector3.UnitZ,
                ZStyle,
                (uint)TransformHandle.Z);
        }
    }
}