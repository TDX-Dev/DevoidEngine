using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering
{
    public class Framebuffer
    {
        IFramebuffer frameBuffer;

        List<Texture2D> RenderTextures;
        Texture2D DepthTexture;

        public Framebuffer()
        {
            frameBuffer = Renderer.graphicsDevice.BufferFactory.CreateFramebuffer();
            RenderTextures = new List<Texture2D>();

        }

        public void Bind()
        {
            frameBuffer.Bind();
        }

        public void Clear()
        {
            frameBuffer.ClearColor(new Vector4(0, 0, 0, 1));
            frameBuffer.ClearDepth(1);
        }

        public Texture2D GetRenderTexture(int index)
        {
            return RenderTextures[index];
        }

        public Texture2D GetDepthTexture()
        {
            return DepthTexture;
        }

        public void Resize(int width, int height)
        {
            frameBuffer = Renderer.graphicsDevice.BufferFactory.CreateFramebuffer();

            for (int i = 0; i < RenderTextures.Count; i++)
            {
                RenderTextures[i].Resize(width, height);
                frameBuffer.AddColorAttachment(RenderTextures[i].GetDeviceTexture());
            }
            DepthTexture.Resize(width, height);
            frameBuffer.AddDepthAttachment(DepthTexture.GetDeviceTexture());
        }

        public void AttachRenderTexture(Texture2D texture)
        {
            frameBuffer.AddColorAttachment(texture.GetDeviceTexture());
            RenderTextures.Add(texture);
        }

        public void AttachDepthTexture(Texture2D texture)
        {
            frameBuffer.AddDepthAttachment(texture.GetDeviceTexture());
            DepthTexture = texture;
        }



    }
}
