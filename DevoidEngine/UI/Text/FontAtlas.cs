using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public sealed class FontAtlas
    {
        public byte[] Pixels = [];

        public int Width;
        public int Height;

        public List<GlyphData> Glyphs = [];
    }
}
