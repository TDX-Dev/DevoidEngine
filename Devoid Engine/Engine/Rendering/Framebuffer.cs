using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering
{
    public class Framebuffer : IDisposable
    {
        private FrameBufferHandle _frameBuffer;

        private List<Texture> RenderTextures;
        private Texture2D? DepthTexture;

        private bool disposed;

        public Framebuffer()
        {
            _frameBuffer = Renderer.ResourceManager.FramebufferManager.CreateFramebuffer();
            RenderTextures = new List<Texture>();

            GPUTracker.FramebufferCount++;
        }

        public void Bind()
        {
            if (disposed) return;

            Renderer.ResourceManager.FramebufferManager.BindFramebuffer(_frameBuffer);
        }

        public void Clear(bool clearDepth = true)
        {
            Vector4 clearColor = new Vector4(0, 0, 0, 1);
            int clearDepthValue = 1;

            Renderer.ResourceManager.FramebufferManager.ClearFramebufferColor(_frameBuffer, clearColor);

            if (clearDepth)
                Renderer.ResourceManager.FramebufferManager.ClearFramebufferDepth(_frameBuffer, clearDepthValue);
        }

        public void Clear(Vector4 clearColor, bool clearDepth = true, int clearDepthValue = 1)
        {
            Renderer.ResourceManager.FramebufferManager.ClearFramebufferColor(_frameBuffer, clearColor);

            if (clearDepth)
                Renderer.ResourceManager.FramebufferManager.ClearFramebufferDepth(_frameBuffer, clearDepthValue);
        }

        public Texture GetRenderTexture(int index)
        {
            return RenderTextures[index];
        }

        public Texture2D? GetDepthTexture()
        {
            return DepthTexture;
        }

        public void Resize(int width, int height)
        {
            if (disposed)
                return;

            // destroy GPU framebuffer
            Renderer.ResourceManager.FramebufferManager.DestroyFramebuffer(_frameBuffer);

            // create new one
            _frameBuffer = Renderer.ResourceManager.FramebufferManager.CreateFramebuffer();

            // reattach textures
            for (int i = 0; i < RenderTextures.Count; i++)
            {
                RenderTextures[i].Resize(width, height);

                Renderer.ResourceManager.FramebufferManager.AttachRenderTexture(
                    _frameBuffer,
                    RenderTextures[i].GetRendererHandle(),
                    i
                );
            }

            if (DepthTexture != null)
            {
                DepthTexture.Resize(width, height);

                Renderer.ResourceManager.FramebufferManager.AttachDepthTexture(
                    _frameBuffer,
                    DepthTexture.GetRendererHandle()
                );
            }
        }

        public void AttachRenderTexture(Texture2D texture)
        {
            Renderer.ResourceManager.FramebufferManager.AttachRenderTexture(
                _frameBuffer,
                texture.GetRendererHandle()
            );

            RenderTextures.Add(texture);
        }

        public void AttachRenderTexture(TextureCube texture, CubeFace face, int mip)
        {
            Renderer.ResourceManager.FramebufferManager.AttachRenderTextureCube(
                _frameBuffer,
                texture.GetRendererHandle(),
                face,
                mip
            );

            RenderTextures.Add(texture);
        }

        public void SetRenderTexture(Texture2D texture, int index = 0)
        {
            Renderer.ResourceManager.FramebufferManager.AttachRenderTexture(
                _frameBuffer,
                texture.GetRendererHandle(),
                index
            );

            if (index >= RenderTextures.Count)
                RenderTextures.Add(texture);
            else
                RenderTextures[index] = texture;
        }

        public void SetRenderTexture(TextureCube texture, CubeFace face, int mip, int index = 0)
        {
            Renderer.ResourceManager.FramebufferManager.AttachRenderTextureCube(
                _frameBuffer,
                texture.GetRendererHandle(),
                face,
                mip,
                index
            );

            if (index >= RenderTextures.Count)
                RenderTextures.Add(texture);
            else
                RenderTextures[index] = texture;
        }

        public void AttachDepthTexture(Texture2D texture)
        {
            Renderer.ResourceManager.FramebufferManager.AttachDepthTexture(
                _frameBuffer,
                texture.GetRendererHandle()
            );

            DepthTexture = texture;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Renderer.ResourceManager.FramebufferManager.DestroyFramebuffer(_frameBuffer);

            RenderTextures.Clear();
            DepthTexture = null;

            GPUTracker.FramebufferCount--;

            GC.SuppressFinalize(this);
        }

        ~Framebuffer()
        {
            Dispose();
        }
    }
}