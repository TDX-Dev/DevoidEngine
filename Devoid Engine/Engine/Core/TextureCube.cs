using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Core
{
    public class TextureCube : Texture
    {
        private TextureHandle _textureInternal;

        public static TextureCube DefaultCube { get; private set; }

        public TextureDescription Description { get; private set; }

        static TextureCube()
        {
            DefaultCube = new TextureCube(new TextureDescription()
            {
                Width = 1,
                Height = 1,
                Format = TextureFormat.RGBA32_Float,
                Type = TextureType.TextureCube,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsDepthStencil = false,
                IsRenderTarget = false,
                IsMutable = false
            });

            // 1x1 neutral gray cubemap
            float[] pixel = [0.5f, 0.5f, 0.5f, 1.0f];

            for (int i = 0; i < 6; i++)
            {
                DefaultCube.SetFaceData((CubeFace)i, pixel);
            }
        }

        public TextureCube(TextureDescription description)
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
            // optional
        }

        public void SetFaceData(CubeFace faceIndex, byte[] data)
        {
            Renderer.ResourceManager.TextureManager.UploadTextureDataCube(_textureInternal, faceIndex, data);
        }

        public void SetFaceData<T>(CubeFace faceIndex, T[] data) where T : unmanaged
        {
            ReadOnlySpan<T> span = data;
            ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(span);

            Renderer.ResourceManager.TextureManager.UploadTextureDataCube(
                _textureInternal,
                faceIndex,
                byteSpan.ToArray()
            );
        }

        public void GenerateMipmaps()
        {
            Renderer.ResourceManager.TextureManager.GenerateMipmaps(_textureInternal);
        }

        public ITextureCube GetDeviceTexture()
        {
            return (ITextureCube)Renderer.ResourceManager.TextureManager.GetDeviceTexture(_textureInternal);
        }

        public override TextureHandle GetRendererHandle()
        {
            return _textureInternal;
        }

        public override void Resize(int width, int height)
        {
            Console.WriteLine("Texture resize for cubemaps not supported yet");
        }

        protected override void DisposeTexture()
        {
            Renderer.ResourceManager.TextureManager.DeleteTexture(_textureInternal);
            GPUTracker.TextureCount--;
        }
    }
}