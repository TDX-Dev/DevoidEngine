using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Core
{
    public class Texture2D : Texture
    {
        private TextureHandle _textureInternal;

        public static Texture2D WhiteTexture { get; private set; }
        public static Texture2D BlackTexture { get; private set; }
        public static ISampler DefaultSampler { get; private set; }

        public TextureDescription Description { get; private set; }

        static Texture2D()
        {
            WhiteTexture = new Texture2D(new TextureDescription()
            {
                Width = 1,
                Height = 1,
                Format = TextureFormat.RGBA32_Float,
                Type = TextureType.Texture2D,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false
            });

            BlackTexture = new Texture2D(new TextureDescription()
            {
                Width = 1,
                Height = 1,
                Format = TextureFormat.RGBA32_Float,
                Type = TextureType.Texture2D,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false
            });

            float[] blackPixel = [
                0,0,0,1
            ];

            float[] whitePixel = [
                1,1,1,1
            ];

            WhiteTexture.SetData(whitePixel);
            BlackTexture.SetData(blackPixel);
            DefaultSampler = Renderer.GraphicsDevice.CreateSampler(SamplerDescription.Default);
        }

        public Texture2D(TextureDescription description)
        {
            Description = description;

            Width = description.Width;
            Height = description.Height;
            _textureInternal = Renderer.ResourceManager.TextureManager.CreateTexture(Description);

            _sampler = Renderer.ResourceManager.SamplerManager.CreateSampler(_samplerDescription);

            GPUTracker.TextureCount++;

        }

        public override void Bind(int slot = 0,
                                  ShaderStage stage = ShaderStage.Fragment,
                                  BindMode mode = BindMode.ReadOnly)
        {
            Renderer.ResourceManager.TextureManager.BindTexture(_textureInternal, slot, stage, mode);

            Renderer.ResourceManager.SamplerManager.BindSampler(_sampler, slot);
        }

        public override void UnBind(int slot)
        {
            //Renderer.ResourceManager.TextureManager.UnBindTexture(_textureInternal, slot);
        }

        public void SetData(byte[] data)
        {
            Renderer.ResourceManager.TextureManager.UploadTextureData2D(_textureInternal, data);
        }

        public void SetData<T>(T[] data) where T : unmanaged
        {
            ReadOnlySpan<T> span = data;
            ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(span);

            Renderer.ResourceManager.TextureManager.UploadTextureData2D(
                _textureInternal,
                byteSpan
            );
        }

        public void GenerateMipmaps()
        {
            Renderer.ResourceManager.TextureManager.GenerateMipmaps(_textureInternal);
        }

        public ITexture2D GetDeviceTexture()
        {
            return (ITexture2D)Renderer.ResourceManager.TextureManager.GetDeviceTexture(_textureInternal);
        }

        public override TextureHandle GetRendererHandle()
        {
            return _textureInternal;
        }

        public override void Resize(int width, int height)
        {
            Renderer.ResourceManager.TextureManager.DeleteTexture(_textureInternal);

            var desc = Description;
            desc.Width = width;
            desc.Height = height;

            Width = width;
            Height = height;

            _textureInternal = Renderer.ResourceManager.TextureManager.CreateTexture(desc);


            Description = desc;
        }

        protected override void DisposeTexture()
        {
            Renderer.ResourceManager.TextureManager.DeleteTexture(_textureInternal);
            GPUTracker.TextureCount--;
        }
    }
}