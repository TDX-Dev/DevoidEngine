using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.UI.Text;
using MessagePack;
using SharpFont;
using System.Runtime.InteropServices;

namespace DevoidEngine.AssetPipeline.Importers
{
    internal class FontImporter : AssetImporter<FontImportSettings>
    {
        public override string Name => "FontImporter";

        public override IReadOnlyList<string> Extensions => [".ttf"];

        public override string OutputExtension => "font";

        public override FontImportSettings DefaultSettings()
        {
            return new FontImportSettings();
        }

        public override void Import(ImportContext context, FontImportSettings settings)
        {
            Library fontLib = Engine.UISystem.FontLibrary.GetLibrary();

            Face fontFace = new(fontLib, context.AssetPath);

            uint scale = (uint)settings.SuperSampleScale;

            fontFace.SetPixelSizes(0, settings.SourceGlyphSize * scale);

            switch (settings.FontEncoding)
            {
                case FontEncodingTypes.Unicode:
                    {
                        fontFace.SelectCharmap(SharpFont.Encoding.Unicode);
                        break;
                    }
            }

            List<GlyphData> glyphs = LoadGlyphs(fontFace, settings);

            Parallel.ForEach(glyphs, glyph =>
            {
                glyph.SDF = SDFGenerator.Generate(
                    glyph.Bitmap,
                    glyph.Width,
                    glyph.Height,
                    glyph.Pitch,
                    (int)settings.PixelRange + 2,
                    (int)settings.PixelRange,
                    settings.SuperSampleScale);

                glyph.BearingX /= scale;
                glyph.BearingY /= scale;
                glyph.Advance /= scale;

                glyph.Bitmap = [];
                glyph.Width = 0;
                glyph.Height = 0;
            });


            FontAtlas atlas = FontAtlasPacker.Build(glyphs);

            FontAsset asset = new()
            {
                ReferenceSize = settings.SourceGlyphSize,
                Ascent = fontFace.Size.Metrics.Ascender.ToSingle() / scale,
                Descent = fontFace.Size.Metrics.Descender.ToSingle() / scale,
                LineHeight = fontFace.Size.Metrics.Height.ToSingle() / scale,
                FontAtlasTexture = atlas.Pixels,
                FontAtlasWidth = atlas.Width,
                FontAtlasHeight = atlas.Height,
                SDFPixelRange = (int)settings.PixelRange,
            };

            foreach (GlyphData glyphData in atlas.Glyphs)
            {
                UI.Text.Glyph glyph = new()
                {
                    Codepoint = glyphData.Codepoint,

                    Advance = glyphData.Advance,

                    BearingX = glyphData.BearingX,
                    BearingY = glyphData.BearingY,

                    Width = glyphData.SDF.Width - glyphData.SDF.Padding * 2,
                    Height = glyphData.SDF.Height - glyphData.SDF.Padding * 2,

                    UVMin = glyphData.UVMin,
                    UVMax = glyphData.UVMax
                };

                asset.Glyphs.Add(glyph.Codepoint, glyph);
            }

            foreach (GlyphData left in glyphs)
            {
                foreach (GlyphData right in glyphs)
                {
                    if (!fontFace.HasKerning)
                        break;

                    FTVector26Dot6 delta = fontFace.GetKerning(
                        left.GlyphIndex,
                        right.GlyphIndex,
                        KerningMode.Default);

                    float amount = delta.X.ToSingle();

                    if (amount != 0)
                    {
                        asset.Kerning.Add(
                            (left.Codepoint, right.Codepoint),
                            amount / settings.SuperSampleScale);
                    }
                }
            }


            //SavePGM(
            //    $"Debug/font_atlas_{Path.GetFileName(context.AssetPath)}.pgm",
            //    atlas.Pixels,
            //    atlas.Width,
            //    atlas.Height
            //);

            File.WriteAllBytes(
                context.GetRootOutputPath(context.OutputExtension),
                MessagePackSerializer.Serialize(asset)
            );
        }

        public override bool Exists(ImportContext context)
        {
            return File.Exists(context.GetRootOutputPath(OutputExtension));
        }

        private static void LoadRange(
            Face face,
            List<GlyphData> glyphs,
            uint first,
            uint last
        )
        {
            for (uint codepoint = first; codepoint <= last; codepoint++)
            {
                GlyphData? glyphData = LoadGlyph(face, codepoint);

                if (glyphData == null)
                    continue;

                glyphs.Add(glyphData);
            }
        }

        private static void LoadCharacters(
            Face face,
            List<GlyphData> glyphs,
            string characters
)
        {
            foreach (char character in characters)
            {
                uint codepoint = (uint)character;

                GlyphData? glyphData = LoadGlyph(face, codepoint);

                if (glyphData == null)
                    continue;

                glyphs.Add(glyphData);
            }
        }

        private static void LoadEntireFont(Face face, List<GlyphData> glyphs)
        {
            uint character = face.GetFirstChar(out uint glyphIndex);

            while (glyphIndex != 0)
            {
                GlyphData? glyphData = LoadGlyph(face, character);

                if (glyphData == null)
                    continue;

                glyphs.Add(glyphData);

                character = face.GetNextChar(character, out glyphIndex);
            }
        }

        private static GlyphData? LoadGlyph(Face face, uint codepoint)
        {
            uint glyphIndex = face.GetCharIndex(codepoint);
            if (glyphIndex == 0)
                return null;

            face.LoadChar(codepoint, LoadFlags.Render, LoadTarget.Normal);

            GlyphSlot slot = face.Glyph;

            GlyphData glyphData = new()
            {
                Codepoint = codepoint,
                GlyphIndex = glyphIndex,

                Width = slot.Bitmap.Width,
                Height = slot.Bitmap.Rows,
                Pitch = slot.Bitmap.Pitch,

                Advance = slot.Advance.X.ToSingle(),

                BearingX = slot.BitmapLeft,
                BearingY = slot.BitmapTop,
            };

            if (slot.Bitmap.Width > 0 && slot.Bitmap.Rows > 0)
            {
                byte[] bitmap = new byte[slot.Bitmap.Width * slot.Bitmap.Rows];

                for (int y = 0; y < slot.Bitmap.Rows; y++)
                {
                    Marshal.Copy(
                        slot.Bitmap.Buffer + y * slot.Bitmap.Pitch,
                        bitmap,
                        y * slot.Bitmap.Width,
                        slot.Bitmap.Width);
                }

                glyphData.Bitmap = bitmap;
            }
            else
            {
                glyphData.Bitmap = [];
            }

            return glyphData;
        }

        private static List<GlyphData> LoadGlyphs(Face face, FontImportSettings settings)
        {
            List<GlyphData> glyphs = [];

            switch (settings.CharacterSet)
            {
                case FontCharacterSet.ASCII:
                    LoadRange(face, glyphs, 32, 126);
                    break;

                case FontCharacterSet.Latin1:
                    LoadRange(face, glyphs, 32, 255);
                    break;

                case FontCharacterSet.Custom:
                    LoadCharacters(face, glyphs, settings.Characters);
                    break;

                case FontCharacterSet.EntireFont:
                    LoadEntireFont(face, glyphs);
                    break;
            }

            return glyphs;
        }

        public static void SavePGM(string path, byte[] pixels, int width, int height)
        {
            using var fs = File.Create(path);
            using var bw = new BinaryWriter(fs);

            bw.Write(System.Text.Encoding.ASCII.GetBytes($"P5\n{width} {height}\n255\n"));
            bw.Write(pixels);
        }
    }
}
