using MessagePack;
using SharpFont;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Text
{
    [MessagePackObject]
    public struct GlyphMetric
    {
        [Key(0)]
        public uint CharCode;

        // The position of the glyph in the atlas (UV coordinates usually calculated from this)
        [Key(1)]
        public int AtlasIndex;

        // The size of the actual pixel data inside the tile
        [Key(2)]
        public int Width;
        [Key(3)]
        public int Height;

        // Positioning logic (Values are normalized to the Atlas Tile Size)
        [Key(4)]
        public float HorizontalAdvance;
        [Key(5)]
        public float BearingX;
        [Key(6)]
        public float BearingY;
        [Key(7)]
        public float OriginalBearingX;
        [Key(8)]
        public float OriginalBearingY;

        [Key(9)]
        public float U;
        [Key(10)]
        public float V;
        [Key(11)]
        public float S;
        [Key(12)]
        public float T;
    }

    public struct BitmapData
    {
        public byte[] Bitmap;
        public int Width;
        public int Height;
    }

    [MessagePackObject]
    public class AtlasData
    {
        [Key(0)]
        public Dictionary<uint, Vector4> GlyphRectangles;
        [Key(1)]
        public byte[] TextureData;
        [Key(2)]
        public Dictionary<uint, GlyphMetric> Metrics;
    }

    public class FontInternal
    {
        string path;
        int pixelSize;

        Face face;


        private const int TargetSpread = 8;
        public const int AtlasTileSize = 64;
        private const int highResSourceSize = 512;

        public readonly float Ascender;
        public readonly float Descender;
        public readonly float LineHeight;

        public Dictionary<uint, GlyphMetric> Metrics { get; private set; }

        public GlyphAtlas Atlas { get; private set; }

        public FontInternal(Library ftLibrary, string path, int size)
        {
            this.path = path;
            this.pixelSize = size;

            face = new Face(ftLibrary, path);
            face.SetPixelSizes(0, highResSourceSize);
            face.SelectCharmap(SharpFont.Encoding.Unicode);

            float scaleFactor = (float)AtlasTileSize / highResSourceSize;

            Ascender = face.Size.Metrics.Ascender.ToSingle() * scaleFactor;
            Descender = face.Size.Metrics.Descender.ToSingle() * scaleFactor;
            LineHeight = face.Size.Metrics.Height.ToSingle() * scaleFactor;

            Metrics = new Dictionary<uint, GlyphMetric>();
            


            ProcessGlyphs();
        }

        public void ProcessGlyphs()
        {
            string debugPath = Path.Combine("./DebugFonts/", face.FamilyName);
            string loadPath = Path.Combine("./RuntimeContent/FontAtlasData/", face.FamilyName);

            float scaleFactor = (float)AtlasTileSize / highResSourceSize;
            uint charCode = 0;

            uint glyphIndex;
            charCode = face.GetFirstChar(out glyphIndex);

            int highResSpread = (int)(TargetSpread / scaleFactor);
            int highResPadding = highResSpread + 5;

            var rawGlyphs = new Dictionary<uint, BitmapData>();

            // ============================================================
            // 🔁 LOAD FROM DISK (FAST PATH)
            // ============================================================
            if (File.Exists(loadPath + ".bin"))
            {
                Atlas = new GlyphAtlas(2048, 2048);
                LoadFromDisk(loadPath + ".bin");
                Atlas.UploadGPU();
                return;
            }

            // ============================================================
            // 🏗 BUILD FROM SCRATCH (ORIGINAL PATH)
            // ============================================================

            while (glyphIndex != 0)
            {
                if (charCode >= 32 && charCode <= 383)
                {
                    var glyphData = GenerateSingleGlyph(charCode, scaleFactor, highResSpread, highResPadding);
                    if (glyphData.Bitmap != null)
                        rawGlyphs[charCode] = glyphData;
                }

                charCode = face.GetNextChar(charCode, out glyphIndex);
            }

            Atlas = new GlyphAtlas(2048, 2048);
            Atlas.Pack(rawGlyphs);
            Atlas.UploadGPU();

            foreach (var kvp in rawGlyphs)
            {
                uint key = kvp.Key;
                if (Atlas.GlyphRectangles.TryGetValue(key, out var rect))
                {
                    var metric = Metrics[key];
                    metric.AtlasIndex = 0;

                    metric.U = rect.X / Atlas.Width;
                    metric.V = rect.Y / Atlas.Height;
                    metric.S = (rect.X + rect.Z) / Atlas.Width;
                    metric.T = (rect.Y + rect.W) / Atlas.Height;

                    Metrics[key] = metric;
                }
            }

            // Optional: Save after building
            SaveToDisk(loadPath + ".bin");
        }

        void SaveToDisk(string path)
        {
            AtlasData data = new AtlasData()
            {
                GlyphRectangles = Atlas.GlyphRectangles,
                TextureData = Atlas.TextureData,
                Metrics = Metrics
            };

            byte[] binData = MessagePackSerializer.Serialize(data);
            File.WriteAllBytes(path, binData);
        }

        void LoadFromDisk(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            AtlasData data = MessagePackSerializer.Deserialize<AtlasData>(fileData);

            Atlas.TextureData = data.TextureData;
            Atlas.GlyphRectangles = data.GlyphRectangles;
            Metrics = data.Metrics;
        }

        private BitmapData GenerateSingleGlyph(uint charCode, float scaleFactor, int spread, int padding)
        {
            face.LoadChar(charCode, LoadFlags.Render | LoadFlags.ForceAutohint, LoadTarget.Normal);
            var ftBmp = face.Glyph.Bitmap;

            if (ftBmp.Width == 0 || ftBmp.Rows == 0)
            {
                Metrics[charCode] = new GlyphMetric
                {
                    CharCode = charCode,
                    Width = 0,
                    Height = 0,
                    HorizontalAdvance = face.Glyph.Metrics.HorizontalAdvance.ToSingle() * scaleFactor,
                    BearingX = 0,
                    BearingY = 0
                };
                return new BitmapData { Bitmap = null, Width = 0, Height = 0 };
            }

            BitmapData sdfBitmapData = SDFGenerator.Generate(
                ftBmp.BufferData,
                ftBmp.Width, ftBmp.Rows, ftBmp.Pitch,
                padding,
                scaleFactor,
                spread
            );

            byte[] sdfBytes = sdfBitmapData.Bitmap;
            int actualW = sdfBitmapData.Width;
            int actualH = sdfBitmapData.Height;

            int dstX = (AtlasTileSize - actualW) / 2;
            int dstY = (AtlasTileSize - actualH) / 2;

            byte[] tileData = new byte[AtlasTileSize * AtlasTileSize];

            int startX = Math.Max(0, dstX);
            int startY = Math.Max(0, dstY);
            int endX = Math.Min(AtlasTileSize, dstX + actualW);
            int endY = Math.Min(AtlasTileSize, dstY + actualH);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int destIndex = y * AtlasTileSize + x;

                    int srcX = x - dstX;
                    int srcY = y - dstY;
                    int srcIndex = srcY * actualW + srcX;

                    tileData[destIndex] = sdfBytes[srcIndex];
                }
            }

            float originalBearingX = face.Glyph.Metrics.HorizontalBearingX.ToSingle() * scaleFactor;
            float originalBearingY = face.Glyph.Metrics.HorizontalBearingY.ToSingle() * scaleFactor;

            float scaledPadding = padding * scaleFactor;

            float finalBearingX = originalBearingX - dstX - scaledPadding;

            float finalBearingY = originalBearingY + dstY + scaledPadding;

            //SaveDebugImage(tileData, AtlasTileSize, AtlasTileSize, "./DebugFonts/Arial/" + charCode + ".png");
            //SaveDebugImage(sdfBytes, actualW, actualH, "./DebugFonts/Arial/" + charCode + ".png");

            Metrics[charCode] = new GlyphMetric
            {
                CharCode = charCode,
                Width = AtlasTileSize,
                Height = AtlasTileSize,
                HorizontalAdvance = (face.Glyph.Metrics.HorizontalAdvance.ToSingle()) * scaleFactor,

                BearingX = finalBearingX,
                OriginalBearingX = originalBearingX,
                OriginalBearingY = originalBearingY,

                BearingY = finalBearingY
            };

            return new BitmapData
            {
                Bitmap = tileData,
                Width = AtlasTileSize,
                Height = AtlasTileSize,
            };
        }

        public float GetScaleForFontSize(float targetPixelSize)
        {

            return targetPixelSize / AtlasTileSize;
        }

        private void SaveDebugImage(byte[] buffer, int w, int h, string path)
        {
            using Image<Rgba32> img = new Image<Rgba32>(w, h);
            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < h; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        byte val = buffer[y * w + x];
                        row[x] = new Rgba32(255, 255, 255, val);
                    }
                }
            });
            img.Save(path);
        }


    }
}