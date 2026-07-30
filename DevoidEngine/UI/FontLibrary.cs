using DevoidEngine.UI.Text;
using SharpFont;

namespace DevoidEngine.UI
{
    public class FontLibrary
    {
        private readonly Library FreeType;
        private readonly Dictionary<string, Font> Fonts = [];

        public FontLibrary()
        {
            FreeType = new Library();
        }

        public Library GetLibrary() => FreeType;

        //public Font LoadFont(string path, int pixelSize)
        //{
        //    var key = path;

        //    if (Fonts.TryGetValue(key, out Font? font))
        //        return font;

        //    font = new Font(FreeType, path);
        //    Fonts[key] = font;

        //    return font;
        //}
    }
}
