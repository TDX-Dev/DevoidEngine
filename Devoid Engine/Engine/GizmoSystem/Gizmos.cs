using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System.Numerics;

namespace DevoidEngine.Engine.GizmoSystem
{
    public static class Gizmos
    {
        static readonly Dictionary<GizmoCategory, Vector4> CategoryColors = new()
        {
            { GizmoCategory.Lighting,   new Vector4(1f, 0.373f, 0.122f, 1) }, // orange
            { GizmoCategory.Physics,    new Vector4(0.2f, 1f, 0.2f, 1f) },  // green
            { GizmoCategory.Cameras,    new Vector4(0.2f, 0.5f, 1f, 1f) },  // blue
            { GizmoCategory.Audio,      new Vector4(0.2f, 0.9f, 1f, 1f) },  // cyan
            { GizmoCategory.AI,         new Vector4(1f, 0.2f, 1f, 1f) },    // magenta
            { GizmoCategory.Gameplay,   new Vector4(1f, 0.5f, 0.2f, 1f) },  // orange
            { GizmoCategory.Navigation, new Vector4(0.9f, 1f, 0.3f, 1f) },  // lime
            { GizmoCategory.Custom,     new Vector4(1f, 1f, 1f, 1f) }       // white
        };

        public static bool Enabled = true;
        public static GizmoCategory EnabledCategories = GizmoCategory.All;

        public static Matrix4x4 Matrix = Matrix4x4.Identity;
        public static Vector4 Color = Vector4.One;

        static bool Allowed(GizmoCategory cat)
        {
            return Enabled && (EnabledCategories & cat) != 0;
        }

        static Vector4 GetCategoryColor(GizmoCategory cat)
        {
            if (CategoryColors.TryGetValue(cat, out var color))
                return color;

            return Vector4.One;
        }

        public static void DrawCube(Matrix4x4 model, GizmoCategory cat)
        {
            if (!Allowed(cat)) return;

            DebugRenderSystem.DrawCube(model, GetCategoryColor(cat));
        }

        public static void DrawCube(Vector3 min, Vector3 max, Matrix4x4 model, GizmoCategory cat)
        {
            if (!Allowed(cat)) return;

            DebugRenderSystem.DrawCube(min, max, model, GetCategoryColor(cat));
        }

        public static void DrawSphere(Matrix4x4 model, GizmoCategory cat)
        {
            if (!Allowed(cat)) return;

            DebugRenderSystem.DrawSphere(model, GetCategoryColor(cat));
        }

        public static void DrawMesh(Mesh mesh, Matrix4x4 model, GizmoCategory cat)
        {
            if (!Allowed(cat)) return;

            DebugRenderSystem.DrawMesh(mesh, model, GetCategoryColor(cat));
        }

        public static void DrawCameraFrustum(Matrix4x4 model, GizmoCategory cat)
        {
            if (!Allowed(cat)) return;

            DebugRenderSystem.DrawMesh(GizmoMeshCache.CameraMesh, model, GetCategoryColor(cat));
        }

        public static void DrawCircle(Matrix4x4 model, GizmoCategory cat)
        {
            if (!Allowed(cat)) return;

            DebugRenderSystem.DrawCircle(model, GetCategoryColor(cat));
        }
    }
}