using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering
{
    public class Framebuffer
    {
        FrameBufferHandle _frameBuffer;

        List<Texture2D> RenderTextures;
        Texture2D DepthTexture;

        public Framebuffer()
        {
            _frameBuffer = Graphics.ResourceManager.FramebufferManager.CreateFramebuffer();
            RenderTextures = new List<Texture2D>();

        }

        public void Bind()
        {
            Graphics.ResourceManager.FramebufferManager.BindFramebuffer(_frameBuffer);
        }

        public void Clear()
        {
            Vector4 clearColor = new Vector4(0, 0, 0, 1);
            int clearDepth = 1;

            Graphics.ResourceManager.FramebufferManager.ClearFramebufferColor(_frameBuffer, clearColor);
            Graphics.ResourceManager.FramebufferManager.ClearFramebufferDepth(_frameBuffer, 1);
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
            _frameBuffer = Graphics.ResourceManager.FramebufferManager.CreateFramebuffer();

            for (int i = 0; i < RenderTextures.Count; i++)
            {
                RenderTextures[i].Resize(width, height);
                Graphics.ResourceManager.FramebufferManager.AttachRenderTexture(_frameBuffer, RenderTextures[i].GetRendererHandle());
            }
            DepthTexture.Resize(width, height);
            Graphics.ResourceManager.FramebufferManager.AttachDepthTexture(_frameBuffer, DepthTexture.GetRendererHandle());
        }

        public void AttachRenderTexture(Texture2D texture)
        {
            Graphics.ResourceManager.FramebufferManager.AttachRenderTexture(_frameBuffer, texture.GetRendererHandle());
            RenderTextures.Add(texture);
        }

        public void AttachDepthTexture(Texture2D texture)
        {
            Graphics.ResourceManager.FramebufferManager.AttachDepthTexture(_frameBuffer, texture.GetRendererHandle());
            DepthTexture = texture;
        }



    }
}
