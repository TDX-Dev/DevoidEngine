using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public enum GizmoDrawCommandType
    {
        Line,
        Arrow,
        Box,
        WireBox,
        Circle,
        Mesh
    }

    public struct GizmoDrawCommand
    {
        public GizmoDrawCommandType Type;

        public Vector3 Start;
        public Vector3 End;

        public Vector3 Center;
        public Vector3 Normal;

        public Vector3 Min;
        public Vector3 Max;

        public float Radius;

        public GizmoMaterial Material;
        public Mesh Mesh;

        public static GizmoDrawCommand Line(
            Vector3 start,
            Vector3 end,
            GizmoMaterial material)
        {
            return new GizmoDrawCommand
            {
                Type = GizmoDrawCommandType.Line,
                Start = start,
                End = end,
                Material = material
            };
        }

        public static GizmoDrawCommand Arrow(
            Vector3 start,
            Vector3 end,
            GizmoMaterial material)
        {
            return new GizmoDrawCommand
            {
                Type = GizmoDrawCommandType.Arrow,
                Start = start,
                End = end,
                Material = material
            };
        }

        public static GizmoDrawCommand Box(
            Vector3 min,
            Vector3 max,
            GizmoMaterial material)
        {
            return new GizmoDrawCommand
            {
                Type = GizmoDrawCommandType.Box,
                Min = min,
                Max = max,
                Material = material
            };
        }

        public static GizmoDrawCommand WireBox(
            Vector3 min,
            Vector3 max,
            GizmoMaterial material)
        {
            return new GizmoDrawCommand
            {
                Type = GizmoDrawCommandType.WireBox,
                Min = min,
                Max = max,
                Material = material
            };
        }

        public static GizmoDrawCommand Circle(
            Vector3 center,
            float radius,
            Vector3 normal,
            GizmoMaterial material)
        {
            return new GizmoDrawCommand
            {
                Type = GizmoDrawCommandType.Circle,
                Center = center,
                Radius = radius,
                Normal = normal,
                Material = material
            };
        }

        public static GizmoDrawCommand MeshType(
            Vector3 position,
            Mesh mesh,
            GizmoMaterial material
        )
        {
            return new GizmoDrawCommand
            {
                Type = GizmoDrawCommandType.Mesh,
                Mesh = mesh,
                Center = position,
                Material = material
            };
        }
    }
}