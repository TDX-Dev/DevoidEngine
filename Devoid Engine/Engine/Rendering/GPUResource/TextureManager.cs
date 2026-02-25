using DevoidEngine.Engine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    = Renderer.graphicsDevice.TextureFactory.CreateTexture(
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
            ITexture textureInternal = _textures[handle.Id];
            RenderThread.Enqueue(() =>
            {
                textureInternal.GenerateMipmaps();
            });
        }

        public void DeleteTexture(TextureHandle handle)
        {
            ITexture textureInternal = _textures[handle.Id];
            RenderThread.Enqueue(() =>
            {
                textureInternal.Dispose();
            });
        }

        public void BindTexture(TextureHandle handle, int slot, ShaderStage stage = ShaderStage.Fragment, BindMode mode = BindMode.ReadOnly)
        {
            Debug.Assert(RenderThread.IsRenderThread());
            if (mode == BindMode.ReadOnly)
                _textures[handle.Id].Bind(slot);
            else
                _textures[handle.Id].BindMutable(slot);

        }

        public void UnBindTexture(TextureHandle handle, int slot, ShaderStage stage = ShaderStage.Fragment, BindMode mode = BindMode.ReadOnly)
        {
            Debug.Assert(RenderThread.IsRenderThread());
            _textures[handle.Id].UnBind(slot);
        }


    }
}
