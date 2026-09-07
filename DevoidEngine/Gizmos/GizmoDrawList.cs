using System.Diagnostics;
using System.Numerics;
using DevoidEngine.Core;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoDrawList
    {
        private readonly List<GizmoDrawCommand> commands = [];

        public IReadOnlyList<GizmoDrawCommand> Commands => commands;

        public bool Enabled { get; set; } = true;

        public GizmoCategory EnabledCategories { get; set; } = GizmoCategory.All;

        public void Clear()
        {
            commands.Clear();
        }

        private bool Allowed(GizmoCategory category)
        {
            return Enabled && (category == GizmoCategory.None || (EnabledCategories & category) != 0);
        }

        public void AddLine(
            Vector3 start,
            Vector3 end,
            Vector4 color)
        {
            AddLine(
                start,
                end,
                GizmoMaterial.Colored(color),
                GizmoCategory.None);
        }

        public void AddLine(
            Vector3 start,
            Vector3 end,
            Vector4 color,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

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

        public void AddLine(
            Vector3 start,
            Vector3 end,
            GizmoMaterial material,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

            AddLine(
                start,
                end,
                material);
        }

        public void AddArrow(
            Vector3 start,
            Vector3 end,
            Vector4 color)
        {
            AddArrow(
                start,
                end,
                GizmoMaterial.Colored(color),
                GizmoCategory.None);
        }

        public void AddArrow(
            Vector3 start,
            Vector3 end,
            Vector4 color,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

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

        public void AddArrow(
            Vector3 start,
            Vector3 end,
            GizmoMaterial material,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

            AddArrow(
                start,
                end,
                material);
        }

        public void AddBox(
            Vector3 min,
            Vector3 max,
            Vector4 color)
        {
            AddBox(
                min,
                max,
                GizmoMaterial.Colored(color),
                GizmoCategory.None);
        }

        public void AddBox(
            Vector3 min,
            Vector3 max,
            Vector4 color,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

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

        public void AddBox(
            Vector3 min,
            Vector3 max,
            GizmoMaterial material,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

            AddBox(
                min,
                max,
                material);
        }

        public void AddWireBox(Vector3 min, Vector3 max, GizmoCategory category)
        {
            if (!Allowed(category))
                return;

            AddWireBox(min, max, GizmoMaterial.Colored(GizmoCategoryColors.Get(category)));
        }

        public void AddWireBox(Vector3 min, Vector3 max, GizmoMaterial material, GizmoCategory category = GizmoCategory.None)
        {
            //Console.WriteLine("Wirebox submitted: Allowed?: " + Allowed(category) + " Frame Index: " + Engine.Instance.FrameCount);
            if (!Allowed(category))
                return;

            commands.Add(GizmoDrawCommand.WireBox(min, max, material));
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
                GizmoMaterial.Colored(color),
                GizmoCategory.None);
        }

        public void AddCircle(
            Vector3 center,
            float radius,
            Vector3 normal,
            Vector4 color,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

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

        public void AddCircle(
            Vector3 center,
            float radius,
            Vector3 normal,
            GizmoMaterial material,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

            AddCircle(
                center,
                radius,
                normal,
                material);
        }

        public void AddMesh(
            Vector3 position,
            Mesh mesh,
            GizmoMaterial material)
        {
            commands.Add(
                GizmoDrawCommand.MeshType(
                    position,
                    mesh,
                    material));
        }

        public void AddMesh(
            Vector3 position,
            Mesh mesh,
            GizmoMaterial material,
            GizmoCategory category)
        {
            if (!Allowed(category))
                return;

            AddMesh(
                position,
                mesh,
                material);
        }
    }
}