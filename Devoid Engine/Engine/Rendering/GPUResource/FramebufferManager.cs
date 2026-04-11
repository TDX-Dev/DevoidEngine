using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Numerics;
using System.Reflection.Metadata;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class FramebufferManager
    {
        private uint _nextFramebufferHandleID = 0;

        internal Dictionary<uint, IFramebuffer> _frameBuffers = new();

        private RenderCommandPool<CreateFramebufferCommand> _createPool = new();
        private RenderCommandPool<BindFramebufferCommand> _bindPool = new();
        private RenderCommandPool<AttachRenderTextureCommand> _attachTexPool = new();
        private RenderCommandPool<AttachRenderTextureCubeCommand> _attachCubePool = new();
        private RenderCommandPool<AttachDepthTextureCommand> _attachDepthPool = new();
        private RenderCommandPool<ClearFramebufferColorCommand> _clearColorPool = new();
        private RenderCommandPool<ClearFramebufferDepthCommand> _clearDepthPool = new();
        private RenderCommandPool<DestroyFramebufferCommand> _destroyPool = new();


        public FrameBufferHandle CreateFramebuffer()
        {
            uint id = ++_nextFramebufferHandleID;
            FrameBufferHandle handle = new(id);

            var cmd = _createPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);

            return handle;
        }


        public void BindFramebuffer(FrameBufferHandle handle)
        {
            var cmd = _bindPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);
        }


        public void AttachRenderTexture(FrameBufferHandle handle, TextureHandle texture, int index = 0)
        {
            var cmd = _attachTexPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Texture = texture;
            cmd.Index = index;

            RenderThread.Enqueue(cmd);
        }


        public void AttachRenderTextureCube(FrameBufferHandle handle, TextureHandle texture, CubeFace faceIndex, int mipLevel = 0, int index = 0)
        {
            var cmd = _attachCubePool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Texture = texture;
            cmd.FaceIndex = faceIndex;
            cmd.MipLevel = mipLevel;
            cmd.Index = index;

            RenderThread.Enqueue(cmd);
        }


        public void AttachDepthTexture(FrameBufferHandle handle, TextureHandle texture)
        {
            var cmd = _attachDepthPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Texture = texture;

            RenderThread.Enqueue(cmd);
        }


        public void ClearFramebufferColor(FrameBufferHandle handle, Vector4 color)
        {
            var cmd = _clearColorPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Color = color;

            RenderThread.Enqueue(cmd);
        }


        public void ClearFramebufferDepth(FrameBufferHandle handle, int value)
        {
            var cmd = _clearDepthPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Value = value;

            RenderThread.Enqueue(cmd);
        }

        public void DestroyFramebuffer(FrameBufferHandle handle)
        {
            var cmd = _destroyPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);
        }
    }
}