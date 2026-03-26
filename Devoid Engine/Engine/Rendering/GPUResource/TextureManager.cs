using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Diagnostics;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class TextureManager
    {
        private uint _nextTextureHandleID = 0;
        private Dictionary<uint, ITexture> _textures = new Dictionary<uint, ITexture>();

        public TextureHandle CreateTexture(TextureDescription textureDescription)
        {
            uint id = ++_nextTextureHandleID;
            TextureHandle textureHandle = new TextureHandle(id);

            RenderThread.Enqueue(() =>
            {
                _textures[textureHandle.Id]
                    = Renderer.GraphicsDevice.TextureFactory.CreateTexture(
                        textureDescription
                    );
            });

            return textureHandle;
        }

        public ITexture GetDeviceTexture(TextureHandle handle)
        {
            Debug.Assert(RenderThread.IsRenderThread());
            return _textures[handle.Id];
        }

        public void UploadTextureData2D(TextureHandle handle, byte[] data)
        {

            RenderThread.Enqueue(() =>
            {
                ITexture2D textureInternal = (ITexture2D)_textures[handle.Id];
                textureInternal.SetData(data);
            });
        }

        public void GenerateMipmaps(TextureHandle handle)
        {
            RenderThread.Enqueue(() =>
            {
                ITexture textureInternal = _textures[handle.Id];
                textureInternal.GenerateMipmaps();
            });
        }

        public void DeleteTexture(TextureHandle handle)
        {
            RenderThread.Enqueue(() =>
            {
                ITexture textureInternal = _textures[handle.Id];
                textureInternal.Dispose();
            });
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
            //_textures[handle.Id].UnBind(slot);
        }


    }
}