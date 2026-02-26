using DevoidEngine.Engine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
                    = Renderer.graphicsDevice.BufferFactory.CreateFramebuffer();
            });
            return handle;
        }

        public void BindFramebuffer(FrameBufferHandle handle)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].Bind();
            });
        }

        public void AttachRenderTexture(FrameBufferHandle handle, TextureHandle texture)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].AddColorAttachment(
                    (ITexture2D)Graphics.ResourceManager.TextureManager.GetDeviceTexture(texture)
                );
            });
        }

        public void AttachDepthTexture(FrameBufferHandle handle, TextureHandle texture)
        {
            RenderThread.Enqueue(() =>
            {
                _frameBuffers[handle.Id].AddDepthAttachment(
                    (ITexture2D)Graphics.ResourceManager.TextureManager.GetDeviceTexture(texture)
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
