using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{
    public static class RenderConstants
    {
        public static Mesh Quad;

        static RenderConstants()
        {
            Quad = new Mesh();
            Quad.SetVertices(Primitives.GetScreenQuadVertex());
        }


    }
}
