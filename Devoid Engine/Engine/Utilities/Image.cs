using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;

namespace DevoidEngine.Engine.Utilities
{
    public class Image
    {
        public int Width, Height;
        public byte[] Pixels = null!;
        public float[] PixelHP = null!;
        public bool IsHDR = false;

        public Image()
        {

        }

        public static (int width, int height, int channels) GetImageDimensions(Stream stream)
        {
            var info = SixLabors.ImageSharp.Image.Identify(stream); // Reads metadata only

            if (info != null)
                return (info.Width, info.Height, info.PixelType.BitsPerPixel / 8);
            else
                throw new Exception("Unable to identify image format.");
        }

        public void LoadPNGAsFloatFromMemory(ReadOnlySpan<byte> data)
        {
            using var stream = new MemoryStream(data.ToArray());

            var image = ImageResultFloat.FromStream(
                stream,
                ColorComponents.RedGreenBlueAlpha
            );

            Width = image.Width;
            Height = image.Height;

            PixelHP = image.Data;
        }

        public void LoadHDRI(string path)
        {
            using var stream = File.OpenRead(path);

            var result = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);

            Width = result.Width;
            Height = result.Height;

            float[] rgb = result.Data;

            float[] rgba = new float[Width * Height * 4];

            for (int i = 0, j = 0; i < rgb.Length; i += 3, j += 4)
            {
                rgba[j + 0] = rgb[i + 0];
                rgba[j + 1] = rgb[i + 1];
                rgba[j + 2] = rgb[i + 2];
                rgba[j + 3] = 1.0f; // alpha
            }

            PixelHP = rgba;
            IsHDR = true;
        }

        //public void Load(byte[] data)
        //{
        //    using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(data);

        //    Width = image.Width;
        //    Height = image.Height;

        //    List<byte> pixels = new List<byte>(4 * image.Width * image.Height);

        //    image.ProcessPixelRows(accessor =>
        //    {
        //        for (int y = 0; y < image.Height; y++)
        //        {
        //            var row = accessor.GetRowSpan(y);

        //            for (int x = 0; x < image.Width; x++)
        //            {
        //                pixels.Add(row[x].R);
        //                pixels.Add(row[x].G);
        //                pixels.Add(row[x].B);
        //                pixels.Add(row[x].A);
        //            }
        //        }
        //    });

        //    Pixels = pixels.ToArray();
        //}

        public void LoadPNG(string path, bool directPath = false)
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

            Width = image.Width;
            Height = image.Height;

            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                        pixels.Add(row[x].A);
                    }
                }
            });

            Pixels = pixels.ToArray();
        }

        public void LoadPNGAsFloat(string path, bool directPath = false)
        {
            using SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

            this.Width = image.Width;
            this.Height = image.Height;

            // 4 floats per pixel
            float[] floatPixels = new float[4 * image.Width * image.Height];

            image.ProcessPixelRows(accessor =>
            {
                int index = 0;
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        var px = row[x];

                        // normalize each channel
                        floatPixels[index++] = px.R / 255f;
                        floatPixels[index++] = px.G / 255f;
                        floatPixels[index++] = px.B / 255f;
                        floatPixels[index++] = px.A / 255f;
                    }
                }
            });

            this.PixelHP = floatPixels;   // store float array here
            this.IsHDR = true;            // mark as HDR/float
            this.Pixels = [];           // not using byte[] now
        }



    }
}