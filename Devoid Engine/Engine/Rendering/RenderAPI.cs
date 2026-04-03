using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{

    public static class RenderAPI
    {
        static RenderAPI()
        {
            var shader = ShaderLibrary.GetShader("Screen/RENDER_SCREEN");
            if (shader == null)
                throw new Exception("Screen shader not loaded");
            screenShader = shader;
            mesh = RenderConstants.Quad;

            var rendererLayout = Renderer.GetInputLayout(mesh, screenShader);
            if (rendererLayout == null)
                throw new Exception("Unable to get layout object from renderer");

            if (mesh == null)
                throw new Exception("Unable to get Quad Mesh from renderer");

            layout = rendererLayout;
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

            var shader = ShaderLibrary.GetShader("Screen/RENDER_SCREEN");

            if (shader == null)
                return;

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(mesh.GetVertices()!.Length, 0);

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

            var shader = ShaderLibrary.GetShader("Screen/RENDER_SCREEN");
            if (shader == null)
                return;

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(mesh.GetVertices()!.Length, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();
        }



        public static void RenderToBuffer(Texture2D texture, Framebuffer destination)
        {
            if (texture == null) return;
            destination.Bind();

            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            Renderer.GraphicsDevice.SetBlendState(BlendMode.AlphaBlend);

            layout.Bind();
            mesh.Bind();

            var shader = ShaderLibrary.GetShader("Screen/RENDER_SCREEN");
            if (shader == null) return;

            texture.BindSampler(0);
            texture.Bind(0);

            Renderer.GraphicsDevice.Draw(mesh.GetVertices()!.Length, 0);

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

            Renderer.GraphicsDevice.Draw(mesh.GetVertices()!.Length, 0);

            Renderer.GraphicsDevice.UnbindAllShaderResources();

        }

        // Assumes you have shader already bound before calling this function.
        public static void RenderFullScreen(Shader shader)
        {
            if (shader == null) return;
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);

            IInputLayout? inputLayout = Renderer.GetInputLayout(mesh, shader);
            if (inputLayout == null) return;

            inputLayout.Bind();
            mesh.Bind();

            Renderer.GraphicsDevice.Draw(mesh.GetVertices()!.Length, 0);
        }

    }
}