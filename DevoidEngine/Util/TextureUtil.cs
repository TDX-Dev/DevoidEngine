using DevoidGPU;
using StbiSharp;
using System.Runtime.InteropServices;

namespace DevoidEngine.Util
{
    public struct ImageData
    {
        public int Width;
        public int Height;
        public TextureFormat Format;
        public byte[] Data;

        public ImageData(int width, int height, TextureFormat format, byte[] data)
        {
            Width = width;
            Height = height;
            Format = format;
            Data = data;
        }
    }

    public static class TextureUtil
    {
        public static float SRGBToLinear(float v)
        {
            if (v <= 0.04045f)
                return v / 12.92f;

            return MathF.Pow((v + 0.055f) / 1.055f, 2.4f);
        }

        public static ImageData LoadImage(string path)
        {
            var data = File.ReadAllBytes(path);
            using var memory = new MemoryStream(data, index: 0, count: data.Length, writable: false, publiclyVisible: true);

            if (Stbi.IsHdrFromMemory(memory))
            {
                using StbiImageF image = Stbi.LoadFFromMemory(memory, 4);

                return new ImageData(
                    image.Width,
                    image.Height,
                    TextureFormat.RGBA32_Float,
                    MemoryMarshal.AsBytes(image.Data).ToArray());
            }
            else
            {
                using StbiImage image = Stbi.LoadFromMemory(memory, 4);

                return new ImageData(
                    image.Width,
                    image.Height,
                    TextureFormat.RGBA8_UNorm,
                    image.Data.ToArray());
            }
        }

        public static ImageData LoadImage(byte[] data)
        {
            using var memory = new MemoryStream(data, index: 0, count: data.Length, writable: false, publiclyVisible: true);

            if (Stbi.IsHdrFromMemory(memory))
            {
                using StbiImageF image = Stbi.LoadFFromMemory(memory, 4);

                return new ImageData(
                    image.Width,
                    image.Height,
                    TextureFormat.RGBA32_Float,
                    MemoryMarshal.AsBytes(image.Data).ToArray());
            }
            else
            {
                using StbiImage image = Stbi.LoadFromMemory(memory, 4);

                return new ImageData(
                    image.Width,
                    image.Height,
                    TextureFormat.RGBA8_UNorm,
                    image.Data.ToArray());
            }
        }



    }
}
