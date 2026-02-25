using DevoidEngine.Engine.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace DevoidEngine.Engine.UI.Text
{
    public class GlyphAtlas
    {
        public Texture2D GPUTexture { get; private set; }
        public byte[] TextureData { get; internal set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Dictionary<uint, Vector4> GlyphRectangles { get; internal set; }

        private const int Padding = 1;

        public GlyphAtlas(int width, int height)
        {
            Width = width;
            Height = height;
            TextureData = new byte[width * height]; // 4 bytes per pixel (RGBA)
            GlyphRectangles = new Dictionary<uint, Vector4>();
        }

        internal void UploadGPU()
        {
            GPUTexture = new Texture2D(new DevoidGPU.TextureDescription()
            {
                Format = DevoidGPU.TextureFormat.R8_UNorm,
                Width = Width,
                Height = Height,
                IsDepthStencil = false,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsRenderTarget = true,
                IsMutable = false,
            });

            GPUTexture.SetFilter(DevoidGPU.TextureFilter.Linear, DevoidGPU.TextureFilter.Linear);

            GPUTexture.SetData(TextureData);
        }

        internal void Pack(Dictionary<uint, BitmapData> glyphs)
        {
            Array.Clear(TextureData, 0, TextureData.Length);
            GlyphRectangles.Clear();

            int currentX = Padding;
            int currentY = Padding;
            int currentRowHeight = 0;

            foreach (var kvp in glyphs)
            {
                uint charCode = kvp.Key;
                BitmapData data = kvp.Value;

                if (currentX + data.Width + Padding > Width)
                {
                    currentX = Padding;
                    currentY += currentRowHeight + Padding; // 🔑 accumulate, don’t reset
                    currentRowHeight = 0;
                }
                if (currentY + data.Height + Padding > Height)
                {
                    Console.WriteLine($"[GlyphAtlas] Atlas Full!");
                    continue;
                }

                CopyPixels(data.Bitmap, data.Width, data.Height, currentX, currentY);

                GlyphRectangles[charCode] = new Vector4(currentX, currentY, data.Width, data.Height);
                currentX += data.Width + Padding;

                if (data.Height > currentRowHeight) currentRowHeight = data.Height;
            }
        }

        private void CopyPixels(byte[] glyphData, int w, int h, int destX, int destY)
        {
            for (int y = 0; y < h; y++)
            {
                // Optimization: Block copy is faster than pixel-by-pixel for simple byte arrays
                // Calculate source offset and destination offset
                int srcOffset = y * w;
                int destOffset = (destY + y) * Width + destX;

                // Copy the row
                Array.Copy(glyphData, srcOffset, TextureData, destOffset, w);
            }
        }

        public void SaveDebug(string path)
        {
            // We still save as RGBA png because most OS viewers can't open raw R8 images
            using (Image<L8> img = new Image<L8>(Width, Height))
            {
                img.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        int rowOffset = y * Width;
                        for (int x = 0; x < Width; x++)
                        {
                            // L8 is a struct wrapping a single byte
                            row[x] = new L8(TextureData[rowOffset + x]);
                        }
                    }
                });
                img.Save(path);
            }
        }

        public void Dispose()
        {
            TextureData = null;
        }

    }
}
