using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{

    public static class RenderAPI
    {
        static RenderAPI()
        {
            screenShader = ShaderLibrary.GetShader("Screen/RENDER_SCREEN");
            mesh = RenderConstants.Quad;

            layout = Renderer.GetInputLayout(mesh, screenShader);
        }

        static Mesh mesh;
        static IInputLayout layout;

        static Shader screenShader;

        // This method should only be called at the end of the render stage
        public static void RenderToScreen(Texture2D texture)
        {
            if (texture == null) return;
            Renderer.GraphicsDevice.MainSurface.Bind();

            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            layout.Bind();
            mesh.Bind();

            ShaderLibrary.GetShader("Screen/RENDER_SCREEN").Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(mesh.GetVertices().Length, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }

        public static void RenderToScreen(TextureCube texture, CubeFace face = CubeFace.PositiveX, int mipLevel = 0)
        {
            if (texture == null) return;
            Renderer.GraphicsDevice.MainSurface.Bind();

            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            layout.Bind();
            mesh.Bind();

            ShaderLibrary.GetShader("Screen/RENDER_SCREEN").Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(mesh.GetVertices().Length, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }



        public static void RenderToBuffer(Texture2D texture, Framebuffer destination)
        {
            if (texture == null) return;
            destination.Bind();

            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);

            layout.Bind();
            mesh.Bind();

            ShaderLibrary.GetShader("Screen/RENDER_SCREEN").Use();

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(mesh.GetVertices().Length, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }

        public static void RenderToBuffer(MaterialInstance material, Framebuffer destination)
        {
            if (material == null) return;
            destination.Bind();

            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            IInputLayout inputLayout = Renderer.GetInputLayout(mesh, material.BaseMaterial.Shader);

            inputLayout.Bind();
            mesh.Bind();

            material.Bind();

            Renderer.GraphicsDevice.Draw(mesh.GetVertices().Length, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();

        }

        // Assumes you have shader already bound before calling this function.
        public static void RenderFullScreen(Shader shader)
        {
            if (shader == null) return;
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            IInputLayout inputLayout = Renderer.GetInputLayout(mesh, shader);

            inputLayout.Bind();
            mesh.Bind();

            Renderer.GraphicsDevice.Draw(mesh.GetVertices().Length, 0);
        }

    }
}