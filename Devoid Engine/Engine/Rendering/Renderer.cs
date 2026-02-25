using DevoidEngine.Engine.Core;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{

    public static class Renderer
    {
        public static IGraphicsDevice graphicsDevice;

        public static int Width;
        public static int Height;

        public static void Initialize(IGraphicsDevice gd, int width, int height)
        {
            Width = width;
            Height = height;

            graphicsDevice = gd;
            graphicsDevice.SetViewport(0, 0, width, height);

            RenderingDefaults.Initialize();
            RenderBase.Initialize(width, height);
        }
        public static void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            RenderBase.Resize(width, height);
        }
        public static IInputLayout GetInputLayout(Mesh mesh, Shader shader)
        {
            var key = (RenderBase._vBuffers[mesh.VertexBuffer.Id].Layout, shader.vShader);
            if (!InputLayoutManager.inputLayoutCache.TryGetValue(key, out var layout))
            {
                layout = Renderer.graphicsDevice.CreateInputLayout(RenderBase._vBuffers[mesh.VertexBuffer.Id].Layout, shader.vShader);
                InputLayoutManager.inputLayoutCache[key] = layout;
            }
            return layout;
        }
    }
}
