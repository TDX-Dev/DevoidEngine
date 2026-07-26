using DevoidEngine.Core;

namespace DevoidEngine.UI.Theme.Styleboxes
{
    public class StyleBoxTexture : StyleBox
    {
        public required Texture Texture;

        public int MarginLeft;
        public int MarginRight;
        public int MarginTop;
        public int MarginBottom;

        public bool TileCenter = false;
    }
}
