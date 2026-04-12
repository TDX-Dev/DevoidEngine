using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;

namespace DevoidEngine.Engine.GizmoSystem
{
    public static class GizmoMeshCache
    {
        public static Mesh ConeMesh;

        static GizmoMeshCache()
        {
            ConeMesh = new Mesh();
            Primitives.GenerateLineCone(16, out Vertex[] coneVertices, out int[] coneIndices);

            ConeMesh.SetVertices(coneVertices);
            ConeMesh.SetIndices(coneIndices);

        }


    }
}
