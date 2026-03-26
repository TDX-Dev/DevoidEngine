using SharpFont;

namespace DevoidEngine.Engine.UI.Text
{
    public static class FontLibrary
    {
        static Library FreeType;
        static Dictionary<(string, int), FontInternal> Fonts = new Dictionary<(string, int), FontInternal>();

        static FontLibrary()
        {
            FreeType = new Library();
        }

        public static FontInternal LoadFont(string path, int pixelSize)
        {
            var key = (path, pixelSize);

            if (Fonts.TryGetValue(key, out FontInternal font))
                return font;

            font = new FontInternal(FreeType, path, pixelSize);
            Fonts[key] = font;

            return font;
        }


    }
}