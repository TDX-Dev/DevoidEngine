using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Diagnostics;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class TextureManager
    {
        private uint _nextTextureHandleID = 0;

        internal Dictionary<uint, ITexture> _textures = new();

        private RenderCommandPool<CreateTextureCommand> _createPool;
        private RenderCommandPool<UploadTextureData2DCommand> _upload2DPool;
        private RenderCommandPool<UploadTextureDataCubeCommand> _uploadCubePool;
        private RenderCommandPool<GenerateMipmapsCommand> _mipmapPool;
        private RenderCommandPool<DeleteTextureCommand> _deletePool;

        public TextureManager()
        {
            _createPool = new();
            _upload2DPool = new();
            _uploadCubePool = new();
            _mipmapPool = new();
            _deletePool = new();
        }


        public TextureHandle CreateTexture(TextureDescription textureDescription)
        {
            uint id = ++_nextTextureHandleID;
            TextureHandle handle = new(id);

            var cmd = _createPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Description = textureDescription;

            RenderThread.Enqueue(cmd);

            return handle;
        }


        public ITexture GetDeviceTexture(TextureHandle handle)
        {
            Debug.Assert(RenderThread.IsRenderThread());
            return _textures[handle.Id];
        }


        public void UploadTextureData2D(TextureHandle handle, byte[] data)
        {
            var cmd = _upload2DPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Data = data;

            RenderThread.Enqueue(cmd);
        }


        public void UploadTextureDataCube(TextureHandle handle, CubeFace face, byte[] data)
        {
            var cmd = _uploadCubePool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Face = face;
            cmd.Data = data;

            RenderThread.Enqueue(cmd);
        }


        public void GenerateMipmaps(TextureHandle handle)
        {
            var cmd = _mipmapPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);
        }


        public void DeleteTexture(TextureHandle handle)
        {
            var cmd = _deletePool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);
        }


        public void BindTexture(TextureHandle handle, int slot, ShaderStage stage = ShaderStage.Fragment, BindMode mode = BindMode.ReadOnly)
        {
            Debug.Assert(RenderThread.IsRenderThread());

            if (mode == BindMode.ReadOnly)
                Renderer.GraphicsDevice.BindTexture(_textures[handle.Id], slot, stage);
            else
                Renderer.GraphicsDevice.BindTextureMutable(_textures[handle.Id], slot);
        }


        public void UnBindTexture(TextureHandle handle, int slot, ShaderStage stage = ShaderStage.Fragment, BindMode mode = BindMode.ReadOnly)
        {
            Debug.Assert(RenderThread.IsRenderThread());
        }
    }
}