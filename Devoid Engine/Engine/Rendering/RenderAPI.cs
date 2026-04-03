using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{
    public static class RenderAPI
    {
        static Mesh mesh;
        static IInputLayout layout;
        static Shader screenShader;
        static int vertexCount;

        static RenderAPI()
        {
            screenShader = ShaderLibrary.GetShader("Screen/RENDER_SCREEN")
                ?? throw new Exception("Screen shader not loaded");

            mesh = RenderConstants.Quad
                ?? throw new Exception("Quad mesh not available");

            layout = Renderer.GetInputLayout(mesh, screenShader)
                ?? throw new Exception("Failed to create input layout");

            vertexCount = mesh.GetVertices()?.Length
                ?? throw new Exception("Mesh vertices missing");
        }

        static void SetupFullscreenState()
        {
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);
        }

        // Render texture directly to the main backbuffer
        public static void RenderToScreen(Texture2D texture)
        {
            if (texture == null) return;

            Renderer.GraphicsDevice.MainSurface.Bind();

            SetupFullscreenState();

            layout.Bind();
            mesh.Bind();

            screenShader.Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(vertexCount, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }

        public static void RenderToScreen(TextureCube texture, CubeFace face = CubeFace.PositiveX, int mipLevel = 0)
        {
            if (texture == null) return;

            Renderer.GraphicsDevice.MainSurface.Bind();

            SetupFullscreenState();

            layout.Bind();
            mesh.Bind();

            screenShader.Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(vertexCount, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }

        // Render texture to framebuffer
        public static void RenderToBuffer(Texture2D texture, Framebuffer destination)
        {
            if (texture == null || destination == null) return;

            destination.Bind();

            SetupFullscreenState();

            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            Renderer.GraphicsDevice.SetBlendState(BlendMode.AlphaBlend);

            layout.Bind();
            mesh.Bind();

            screenShader.Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(vertexCount, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }

        // Render material to framebuffer
        public static void RenderToBuffer(MaterialInstance material, Framebuffer destination)
        {
            if (material == null || destination == null) return;

            destination.Bind();

            SetupFullscreenState();

            IInputLayout inputLayout = Renderer.GetInputLayout(mesh, material.BaseMaterial.Shader);
            if (inputLayout == null) return;

            inputLayout.Bind();
            mesh.Bind();

            material.Bind();

            Renderer.GraphicsDevice.Draw(vertexCount, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }

        // Render fullscreen with already bound shader
        public static void RenderFullScreen(Shader shader)
        {
            if (shader == null) return;

            SetupFullscreenState();

            IInputLayout inputLayout = Renderer.GetInputLayout(mesh, shader);
            if (inputLayout == null) return;

            inputLayout.Bind();
            mesh.Bind();

            Renderer.GraphicsDevice.Draw(vertexCount, 0);
        }
    }
}