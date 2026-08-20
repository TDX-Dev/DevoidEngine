using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class RenderMeshData
    {
        public Mesh render_mesh = null!;
        public Matrix4x4 render_transform;
        public MaterialInstance? render_material;

        public bool is_static = false;

        public RenderMeshData() { }

        public RenderMeshData(Mesh mesh)
        {
            render_mesh = mesh;
        }
    }
}
