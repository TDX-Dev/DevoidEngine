using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using DevoidGPU;
using MessagePack;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class TextureLoader : IAssetLoader<Texture2D>
    {
        public Texture2D Load(ReadOnlySpan<byte> data)
        {
            TextureAsset asset;

            try
            {
                asset = MessagePackSerializer.Deserialize<TextureAsset>(data.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Texture Loader]: Error Loading Texture {e.Message}");
                return Texture2D.WhiteTexture;
            }

            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = asset.Width,
                Height = asset.Height,
                Format = asset.Format,
                GenerateMipmaps = asset.GenerateMipmaps,
                MipLevels = asset.GenerateMipmaps ? 0 : 1,
                IsDepthStencil = false,
                IsRenderTarget = asset.GenerateMipmaps,
                IsMutable = false
            });

            texture.SetFilter(asset.Filter, asset.Filter);
            texture.SetWrapMode(asset.Wrap, asset.Wrap);
            texture.SetAnisotropy(asset.Anisotropy);

            Console.WriteLine("Loaded Texture!");

            switch (asset.Format)
            {
                case TextureFormat.RGBA8_UNorm:
                    {
                        texture.SetData(asset.PixelData);
                        break;
                    }

                case TextureFormat.RGBA16_Float:
                    {
                        Half[] halfPixels = MemoryMarshal.Cast<byte, Half>(asset.PixelData).ToArray();
                        texture.SetData(halfPixels);
                        break;
                    }

                case TextureFormat.RGBA32_Float:
                    {
                        float[] floatPixels = MemoryMarshal.Cast<byte, float>(asset.PixelData).ToArray();
                        texture.SetData(floatPixels);
                        break;
                    }

                default:
                    throw new NotSupportedException($"Unsupported texture format {asset.Format}");
            }

            if (asset.GenerateMipmaps)
            {
                texture.GenerateMipmaps();
                Console.WriteLine("Generated Mips!");
            }

            return texture;
        }
    }
}