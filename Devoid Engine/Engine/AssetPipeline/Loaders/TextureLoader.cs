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
            var asset = MessagePackSerializer.Deserialize<TextureAsset>(data.ToArray());
            if (asset.PixelData.Length != asset.Width * asset.Height * 4)
                throw new Exception("Invalid pixel count");
            Texture2D texture = new Texture2D(new TextureDescription()
            {
                Width = asset.Width,
                Height = asset.Height,
                Format = asset.Format,
                GenerateMipmaps = false,
                IsDepthStencil = false,
                IsRenderTarget = false,
                IsMutable = false
            });

            texture.SetFilter(asset.Filter, asset.Filter);
            texture.SetAnisotropy(asset.Anisotropy);
            texture.SetWrapMode(asset.Wrap, asset.Wrap);
            texture.SetData(asset.PixelData);

            return texture;
        }
    }
}