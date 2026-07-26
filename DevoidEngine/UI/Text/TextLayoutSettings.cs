using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Text
{
    public struct TextLayoutSettings
    {
        public float FontSize;

        public float MaxWidth;
        public float MaxHeight;

        public TextOverflow Overflow;

        public TextHorizontalAlignment HorizontalAlignment;
        public TextVerticalAlignment VerticalAlignment;
    }
}
