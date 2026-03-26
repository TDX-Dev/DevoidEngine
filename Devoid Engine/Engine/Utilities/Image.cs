using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;

namespace DevoidEngine.Engine.Utilities
{
    public class Image
    {
        public int Width, Height;
        public byte[] Pixels;
        public float[] PixelHP;
        public bool IsHDR;

        public Image()
        {

        }

        public static (int width, int height, int channels) GetImageDimensions(Stream stream)
        {
            IImageFormat format;
            var info = SixLabors.ImageSharp.Image.Identify(stream); // Reads metadata only

            if (info != null)
                return (info.Width, info.Height, info.PixelType.BitsPerPixel / 8);
            else
                throw new Exception("Unable to identify image format.");
        }

        public void LoadHDRI(string path, bool directPath = false)
        {
            using (var stream = File.OpenRead(path))
            {
                var imageResult = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);

                this.Width = imageResult.Width;
                this.Height = imageResult.Height;
                this.PixelHP = imageResult.Data;
                this.IsHDR = true;

                this.Pixels = null;
            }
        }

        public void Load(byte[] data)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(data);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[4 * image.Width * image.Height];
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

        public void LoadPNG(string path, bool directPath = false)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[4 * image.Width * image.Height];
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
            this.Pixels = null;           // not using byte[] now
        }



    }
}