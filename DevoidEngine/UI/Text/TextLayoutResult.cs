using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public class TextLayoutResult
    {
        public List<PositionedGlyph> Glyphs = [];

        public List<int> LineStarts = [];
        public List<float> LineWidths = [];

        public float Width;
        public float Height;

        public int LineCount;
    }
}
