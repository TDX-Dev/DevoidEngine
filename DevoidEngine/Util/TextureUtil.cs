using DevoidGPU;
using StbiSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    public readonly struct ImageData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly TextureFormat Format;
        public readonly byte[] Data;

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
            using var stream = File.OpenRead(path);
            using var memory = new MemoryStream();

            stream.CopyTo(memory);

            using StbiImage image = Stbi.LoadFromMemory(memory, 4);

            return new ImageData(
                image.Width,
                image.Height,
                TextureFormat.RGBA8_UNorm,
                image.Data.ToArray() // byte[]
            );
        }

        public static ImageData LoadImage(byte[] data)
        {
            using var memory = new MemoryStream(
                data,
                index: 0,
                count: data.Length,
                writable: false,
                publiclyVisible: true);

            using StbiImage image = Stbi.LoadFromMemory(memory, 4);

            return new ImageData(
                image.Width,
                image.Height,
                TextureFormat.RGBA8_UNorm,
                image.Data.ToArray()
            );
        }



    }
}
