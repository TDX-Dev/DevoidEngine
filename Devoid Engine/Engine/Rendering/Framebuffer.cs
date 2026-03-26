using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering.GPUResource;
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
            _frameBuffer = Renderer.ResourceManager.FramebufferManager.CreateFramebuffer();
            RenderTextures = new List<Texture2D>();

        }

        public void Bind()
        {
            Renderer.ResourceManager.FramebufferManager.BindFramebuffer(_frameBuffer);
        }

        public void Clear()
        {
            Vector4 clearColor = new Vector4(0, 0, 0, 1);
            int clearDepth = 1;

            Renderer.ResourceManager.FramebufferManager.ClearFramebufferColor(_frameBuffer, clearColor);
            Renderer.ResourceManager.FramebufferManager.ClearFramebufferDepth(_frameBuffer, 1);
        }

        public void Clear(Vector4 clearColor, int clearDepth = 1)
        {
            Renderer.ResourceManager.FramebufferManager.ClearFramebufferColor(_frameBuffer, clearColor);
            Renderer.ResourceManager.FramebufferManager.ClearFramebufferDepth(_frameBuffer, 1);
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
            _frameBuffer = Renderer.ResourceManager.FramebufferManager.CreateFramebuffer();

            for (int i = 0; i < RenderTextures.Count; i++)
            {
                RenderTextures[i].Resize(width, height);
                Renderer.ResourceManager.FramebufferManager.AttachRenderTexture(_frameBuffer, RenderTextures[i].GetRendererHandle());
            }
            if (DepthTexture != null)
            {
                DepthTexture.Resize(width, height);
                Renderer.ResourceManager.FramebufferManager.AttachDepthTexture(_frameBuffer, DepthTexture.GetRendererHandle());
            }
        }

        public void AttachRenderTexture(Texture2D texture)
        {
            Renderer.ResourceManager.FramebufferManager.AttachRenderTexture(_frameBuffer, texture.GetRendererHandle());
            RenderTextures.Add(texture);
        }

        public void SetRenderTexture(Texture2D texture, int index = 0)
        {
            Renderer.ResourceManager.FramebufferManager.AttachRenderTexture(_frameBuffer, texture.GetRendererHandle(), index);
            if (index >= RenderTextures.Count)
            {
                RenderTextures.Add(texture);
            }
            else
            {
                RenderTextures[index] = texture;
            }
        }

        public void AttachDepthTexture(Texture2D texture)
        {
            Renderer.ResourceManager.FramebufferManager.AttachDepthTexture(_frameBuffer, texture.GetRendererHandle());
            DepthTexture = texture;
        }



    }
}