using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.UI.Text;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class FontLoader : IAssetLoader<Font>
    {
        public Font Load(ReadOnlySpan<byte> data)
        {
            FontAsset asset;

            try
            {
                asset = MessagePackSerializer.Deserialize<FontAsset>(data.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Font Loader]: Error Loading Font {e.Message}");
                throw new Exception();
            }

            Font font = new()
            {
                Ascent = asset.Ascent,
                Descent = asset.Descent,
                Glyphs = asset.Glyphs,
                Kerning = asset.Kerning,
                LineHeight = asset.LineHeight,
                ReferenceSize = asset.ReferenceSize,
                FontAtlasTexture = Texture.Create2D(asset.FontAtlasWidth, asset.FontAtlasHeight, DevoidGPU.TextureFormat.R8_UNorm, DevoidGPU.TextureUsage.ShaderResource),
                SDFPixelRange = asset.SDFPixelRange,
            };
            font.FontAtlasTexture.GPU.Update(asset.FontAtlasTexture);
            return font;
        }
    }
}
