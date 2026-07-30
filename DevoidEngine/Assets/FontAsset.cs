using DevoidEngine.UI.Text;
using MessagePack;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class FontAsset
    {
        [Key(0)]
        public float ReferenceSize;
        [Key(1)]
        public float Ascent;
        [Key(2)]
        public float Descent;
        [Key(3)]
        public float LineHeight;
        [Key(4)]
        public byte[] FontAtlasTexture = [];
        [Key(5)]
        public int FontAtlasWidth;
        [Key(6)]
        public int FontAtlasHeight;
        [Key(7)]
        public Dictionary<uint, Glyph> Glyphs = [];
        [Key(8)]
        public Dictionary<(uint, uint), float> Kerning = [];
        [Key(9)]
        public int SDFPixelRange;


    }
}
