using Assimp;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidGPU;
using MessagePack;
using System.Runtime.InteropServices;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class TextureLoader : IAssetLoader<Texture>
    {
        public string RuntimeExtension => "texture";
        public Texture Load(byte[] data)
        {

            TextureAsset asset;

            try
            {
                asset = MessagePackSerializer.Deserialize<TextureAsset>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Texture Loader]: Error Loading Texture {e.Message}");
                return Texture.Default;
            }

            TextureDescription description = new()
            {
                ArraySize = 1,
                Depth = 1,
                Width = asset.Width,
                Height = asset.Height,
                Dimension = TextureDimension.Texture2D,
                Format = asset.Format,
                MipLevels = asset.GenerateMipmaps ? (int)(Math.Log2(asset.Width) + 1) : 1,
                Samples = new TextureSampleDescription(1, 0),
                Usage = TextureUsage.ShaderResource,
            };

            if (asset.GenerateMipmaps)
                description.Usage |= TextureUsage.RenderTarget;

            Texture texture = new(description);

            //Texture texture = new Texture2D(new TextureDescription()
            //{
            //    Width = asset.Width,
            //    Height = asset.Height,
            //    Format = asset.Format,
            //    GenerateMipmaps = asset.GenerateMipmaps,
            //    MipLevels = asset.GenerateMipmaps ? 0 : 1,
            //    IsDepthStencil = false,
            //    IsRenderTarget = asset.GenerateMipmaps,
            //    IsMutable = false
            //});

            //texture.SetFilter(asset.Filter, asset.Filter);
            //texture.SetWrapMode(asset.Wrap, asset.Wrap);
            //texture.SetAnisotropy(asset.Anisotropy);

            switch (asset.Format)
            {
                case TextureFormat.RGBA8_UNorm:
                case TextureFormat.RGBA8_UNorm_SRGB:
                    {
                        texture.GPU.Update(asset.PixelData);
                        break;
                    }

                case TextureFormat.RGBA16_Float:
                    {
                        ReadOnlySpan<Half> halfPixels = MemoryMarshal.Cast<byte, Half>(asset.PixelData);
                        texture.GPU.Update(halfPixels);
                        break;
                    }

                case TextureFormat.RGBA32_Float:
                    {
                        ReadOnlySpan<float> floatPixels = MemoryMarshal.Cast<byte, float>(asset.PixelData);
                        texture.GPU.Update(floatPixels);
                        break;
                    }

                default:
                    throw new NotSupportedException($"Unsupported texture format {asset.Format}");
            }

            if (asset.GenerateMipmaps)
            {
                Engine.Renderer.EnqueueGPUCommand(cmd => cmd.GenerateMipmaps(texture.GPU));
            }

            return texture;
        }
    }
}
