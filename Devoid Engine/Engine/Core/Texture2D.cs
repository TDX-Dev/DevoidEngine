using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Core
{
    public class Texture2D : Texture
    {
        private TextureHandle _textureInternal;

        public static Texture2D WhiteTexture { get; private set; }
        public static ISampler DefaultSampler { get; private set; }

        public TextureDescription Description { get; private set; }

        static Texture2D()
        {
            WhiteTexture = new Texture2D(new TextureDescription()
            {
                Width = 1,
                Height = 1,
                Format = TextureFormat.RGBA16_Float,
                Type = TextureType.Texture2D,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false
            });
            DefaultSampler = Renderer.graphicsDevice.CreateSampler(SamplerDescription.Default);
        }

        public Texture2D(TextureDescription description)
        {
            Description = description;

            Width = description.Width;
            Height = description.Height;

            _textureInternal = Graphics.ResourceManager.TextureManager.CreateTexture(Description);

            _sampler = Graphics.ResourceManager.SamplerManager.CreateSampler(_samplerDescription);
        }

        public override void Bind(int slot = 0,
                                  ShaderStage stage = ShaderStage.Fragment,
                                  BindMode mode = BindMode.ReadOnly)
        {
            Graphics.ResourceManager.TextureManager.BindTexture(_textureInternal, slot, stage, mode);

            Graphics.ResourceManager.SamplerManager.BindSampler(_sampler, slot);
        }

        public override void UnBind(int slot)
        {
            Graphics.ResourceManager.TextureManager.UnBindTexture(_textureInternal, slot);
        }

        public void SetData(byte[] data)
        {
            Graphics.ResourceManager.TextureManager.UploadTextureData2D(_textureInternal, data);
        }

        public void SetData<T>(T[] data) where T : unmanaged
        {
            ReadOnlySpan<T> span = data;
            ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(span);
            Graphics.ResourceManager.TextureManager.UploadTextureData2D(_textureInternal, byteSpan.ToArray());
        }

        public void GenerateMipmaps()
        {
            Graphics.ResourceManager.TextureManager.GenerateMipmaps(_textureInternal);
        }

        public ITexture2D GetDeviceTexture()
        {
            return (ITexture2D)Graphics.ResourceManager.TextureManager.GetDeviceTexture(_textureInternal);
        }

        public void Resize(int width, int height)
        {
            Graphics.ResourceManager.TextureManager.DeleteTexture(_textureInternal);

            var desc = Description;
            desc.Width = width;
            desc.Height = height;

            Width = width;
            Height = height;

            _textureInternal = Graphics.ResourceManager.TextureManager.CreateTexture(desc);


            Description = desc;
        }
    }
}
