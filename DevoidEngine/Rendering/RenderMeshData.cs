using DevoidEngine.Core;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class RenderMeshData
    {
        public Mesh RenderMesh = null!;
        public Matrix4x4 RenderTransform;
        public MaterialInstance? RenderMaterial;

        public bool IsStatic = false;

        public RenderMeshData() { }

        public RenderMeshData(Mesh mesh)
        {
            RenderMesh = mesh;
        }
    }
}
