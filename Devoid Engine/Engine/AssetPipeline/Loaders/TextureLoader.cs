using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using DevoidGPU;
using MessagePack;
using SixLabors.ImageSharp.PixelFormats;

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
            } catch (Exception e)
            {
                Console.WriteLine($"[Texture Loader]: Error Loading Texture {e.Message}");
                return Texture2D.WhiteTexture;
            }
            if (asset.PixelData.Length != asset.Width * asset.Height * 4)
                throw new Exception("Invalid pixel count");
            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = asset.Width,
                Height = asset.Height,
                Format = asset.Format,
                GenerateMipmaps = false,
                IsDepthStencil = false,
                MipLevels = 1,
                IsRenderTarget = false,
                IsMutable = false
            });

            texture.SetFilter(asset.Filter, asset.Filter);
            texture.SetAnisotropy(asset.Anisotropy);
            texture.SetWrapMode(asset.Wrap, asset.Wrap);
            texture.SetData(asset.PixelData);

            Console.WriteLine("Texture was loaded");

            return texture;
        }
    }
}