using System.Numerics;
using DevoidEngine.Core;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoDrawList
    {
        private readonly List<GizmoDrawCommand> commands = [];

        public IReadOnlyList<GizmoDrawCommand> Commands => commands;

        public void Clear()
        {
            commands.Clear();
        }

        public void AddLine(
            Vector3 start,
            Vector3 end,
            Vector4 color)
        {
            AddLine(
                start,
                end,
                GizmoMaterial.Colored(color));
        }

        public void AddLine(
            Vector3 start,
            Vector3 end,
            GizmoMaterial material)
        {
            commands.Add(
                GizmoDrawCommand.Line(
                    start,
                    end,
                    material));
        }

        public void AddArrow(
            Vector3 start,
            Vector3 end,
            Vector4 color)
        {
            AddArrow(
                start,
                end,
                GizmoMaterial.Colored(color));
        }

        public void AddArrow(
            Vector3 start,
            Vector3 end,
            GizmoMaterial material)
        {
            commands.Add(
                GizmoDrawCommand.Arrow(
                    start,
                    end,
                    material));
        }

        public void AddBox(
            Vector3 min,
            Vector3 max,
            Vector4 color)
        {
            AddBox(
                min,
                max,
                GizmoMaterial.Colored(color));
        }

        public void AddBox(
            Vector3 min,
            Vector3 max,
            GizmoMaterial material)
        {
            commands.Add(
                GizmoDrawCommand.Box(
                    min,
                    max,
                    material));
        }

        public void AddWireBox(
            Vector3 min,
            Vector3 max,
            Vector4 color)
        {
            AddWireBox(
                min,
                max,
                GizmoMaterial.Colored(color));
        }

        public void AddWireBox(
            Vector3 min,
            Vector3 max,
            GizmoMaterial material)
        {
            commands.Add(
                GizmoDrawCommand.WireBox(
                    min,
                    max,
                    material));
        }

        public void AddCircle(
            Vector3 center,
            float radius,
            Vector3 normal,
            Vector4 color)
        {
            AddCircle(
                center,
                radius,
                normal,
                GizmoMaterial.Colored(color));
        }

        public void AddCircle(
            Vector3 center,
            float radius,
            Vector3 normal,
            GizmoMaterial material)
        {
            commands.Add(
                GizmoDrawCommand.Circle(
                    center,
                    radius,
                    normal,
                    material));
        }

        public void AddMesh(
            Vector3 position,
            Mesh mesh,
            GizmoMaterial material
        )
        {
            commands.Add(
                GizmoDrawCommand.MeshType(
                    position,
                    mesh,
                    material));
        }
    }
}