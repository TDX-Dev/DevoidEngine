using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.UI.Text
{


    internal class FontAtlasPacker
    {
        public static FontAtlas Build(
        List<GlyphData> glyphs)
        {

            int totalArea = glyphs.Sum(g => g.SDF.Width * g.SDF.Height);

            float efficiency = 0.90f;

            int estimatedArea = (int)MathF.Ceiling(totalArea / efficiency);
            int estimatedSide = (int)MathF.Ceiling(MathF.Sqrt(estimatedArea));

            int size = (int)BitOperations.RoundUpToPowerOf2((uint)estimatedSide);
            int atlasWidth = size;
            int atlasHeight = size;



            glyphs.Sort((a, b) =>
            {
                int areaA = a.SDF.Width * a.SDF.Height;
                int areaB = b.SDF.Width * b.SDF.Height;

                int result = areaB.CompareTo(areaA);

                if (result != 0)
                    return result;

                return b.SDF.Height.CompareTo(a.SDF.Height);
            });

            SkylinePacker packer = new(atlasWidth, atlasHeight);

            foreach (GlyphData glyph in glyphs)
            {
                if (!packer.TryPack(
                        glyph.SDF.Width,
                        glyph.SDF.Height,
                        out int x,
                        out int y))
                {
                    throw new Exception("Font atlas is too small.");
                }

                glyph.AtlasX = x;
                glyph.AtlasY = y;
            }

            int usedHeight = 0;

            foreach (GlyphData glyph in glyphs)
            {
                usedHeight = Math.Max(
                    usedHeight,
                    glyph.AtlasY + glyph.SDF.Height);
            }
            atlasHeight = usedHeight;

            byte[] atlasPixels = new byte[atlasWidth * atlasHeight];


            foreach (GlyphData glyph in glyphs)
            {
                CopyGlyph(atlasPixels, atlasWidth, glyph);
            }


            foreach (GlyphData glyph in glyphs)
            {
                int p = glyph.SDF.Padding;

                glyph.UVMin = new Vector2(
                    (glyph.AtlasX + p) / (float)atlasWidth,
                    (glyph.AtlasY + p) / (float)atlasHeight);

                glyph.UVMax = new Vector2(
                    (glyph.AtlasX + glyph.SDF.Width - p) / (float)atlasWidth,
                    (glyph.AtlasY + glyph.SDF.Height - p) / (float)atlasHeight);
            }

            return new FontAtlas
            {
                Width = atlasWidth,
                Height = atlasHeight,
                Pixels = atlasPixels,
                Glyphs = glyphs
            };

        }

        private static void CopyGlyph(
            byte[] atlas,
            int atlasWidth,
            GlyphData glyph
        )
        {
            byte[] source = glyph.SDF.SDF;

            int sourceWidth = glyph.SDF.Width;
            int sourceHeight = glyph.SDF.Height;

            for (int row = 0; row < sourceHeight; row++)
            {
                Buffer.BlockCopy(
                    source,
                    row * sourceWidth,

                    atlas,
                    (glyph.AtlasY + row) * atlasWidth + glyph.AtlasX,

                    sourceWidth);
            }
        }
    }
}
