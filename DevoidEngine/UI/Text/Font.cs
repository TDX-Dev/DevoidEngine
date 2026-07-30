using DevoidEngine.Core;

namespace DevoidEngine.UI.Text
{
    public class Font
    {
        public float ReferenceSize;
        public float Ascent;
        public float Descent;
        public float LineHeight;

        public int SDFPixelRange;

        public Texture FontAtlasTexture = null!;
        public int FontAtlasWidth;

        public int FontAtlasHeight;

        public Dictionary<uint, Glyph> Glyphs = [];
        public Dictionary<(uint, uint), float> Kerning = [];

        public Font()
        {

        }
    }
}
