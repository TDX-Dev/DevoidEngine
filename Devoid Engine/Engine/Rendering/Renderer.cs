using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public static class Renderer
    {
        public static IGraphicsDevice GraphicsDevice { get; internal set; }
        internal static ResourceManager ResourceManager = new ResourceManager();
        internal static InputLayoutCache inputLayoutCache = new InputLayoutCache();

        public static IInputLayout GetInputLayout(Mesh mesh, Shader shader) => inputLayoutCache.Get(GraphicsDevice, mesh.VertexBuffer.GetVertexInfo(), shader.vShader);

        public static void Render(CameraRenderContext cameraRenderContext)
        {

        }

        public static void ExecuteDrawList()
        {

        }


    }
}
