using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Theme.Styleboxes
{
    public class StyleBoxTexture : StyleBox
    {
        public required Texture2D Texture;

        public int MarginLeft;
        public int MarginRight;
        public int MarginTop;
        public int MarginBottom;

        public bool TileCenter = false;
    }
}
