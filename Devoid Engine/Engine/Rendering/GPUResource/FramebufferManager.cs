using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class FramebufferManager
    {
        private uint _nextFramebufferHandleID = 0;
        private Dictionary<uint, IFramebuffer> _frameBuffers = new Dictionary<uint, IFramebuffer>();


        public FrameBufferHandle CreateFramebuffer()
        {
            uint id = ++_nextFramebufferHandleID;
            FrameBufferHandle handle = new FrameBufferHandle(id);
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[id]
                    = Renderer.GraphicsDevice.BufferFactory.CreateFramebuffer();
            });
            return handle;
        }

        public void BindFramebuffer(FrameBufferHandle handle)
        {
            RenderThread.Enqueue(() =>
            {
                Renderer.GraphicsDevice.BindFramebuffer(_frameBuffers[handle.Id]);
            });
        }

        public void AttachRenderTexture(FrameBufferHandle handle, TextureHandle texture, int index = 0)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].AddColorAttachment(
                    (ITexture2D)Renderer.ResourceManager.TextureManager.GetDeviceTexture(texture),
                    index
                );
            });
        }

        public void AttachDepthTexture(FrameBufferHandle handle, TextureHandle texture)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].AddDepthAttachment(
                    (ITexture2D)Renderer.ResourceManager.TextureManager.GetDeviceTexture(texture)
                );
            });
        }

        public void ClearFramebufferColor(FrameBufferHandle handle, Vector4 color)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].ClearColor(color);
            });
        }

        public void ClearFramebufferDepth(FrameBufferHandle handle, int value)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].ClearDepth(value);
            });
        }

    }
}