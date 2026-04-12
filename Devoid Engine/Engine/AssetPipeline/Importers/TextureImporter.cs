using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using MessagePack;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public class TextureImporter : AssetImporter<TextureImportSettings>
    {
        public override string Name => "TextureImporter";

        public override IReadOnlyList<string> Extensions =>
            new[] { ".png", ".jpg", ".jpeg", ".hdr" };

        public override string OutputExtension => "texture";

        public override int Priority => 100;

        public override TextureImportSettings DefaultSettings()
        {
            return new TextureImportSettings();
        }

        public override void Import(
            string assetPath,
            Guid guid,
            TextureImportSettings settings,
            string outputPath)
        {
            Console.WriteLine($"Importing texture {assetPath}");

            byte[] fileBytes = File.ReadAllBytes(assetPath);

            Helper.LoadImageFloat(fileBytes, out int width, out int height, out float[] data);

            byte[] pixels = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 4)
            {
                float r = data[i];
                float g = data[i + 1];
                float b = data[i + 2];
                float a = data[i + 3];

                if (settings.SRGB)
                {
                    r = Helper.SRGBToLinear(r);
                    g = Helper.SRGBToLinear(g);
                    b = Helper.SRGBToLinear(b);
                }

                pixels[i] = (byte)(Math.Clamp(r, 0f, 1f) * 255f);
                pixels[i + 1] = (byte)(Math.Clamp(g, 0f, 1f) * 255f);
                pixels[i + 2] = (byte)(Math.Clamp(b, 0f, 1f) * 255f);
                pixels[i + 3] = (byte)(Math.Clamp(a, 0f, 1f) * 255f);
            }

            var asset = new TextureAsset
            {
                Width = width,
                Height = height,
                Format = settings.Format,
                Anisotropy = 8,
                Filter = settings.Filter,
                Wrap = settings.Wrap,
                PixelData = pixels
            };

            File.WriteAllBytes(
                outputPath,
                MessagePackSerializer.Serialize(asset)
            );
        }

        //public override void Import(
        //    string assetPath,
        //    Guid guid,
        //    TextureImportSettings settings,
        //    string outputPath
        //)
        //{
        //    Console.WriteLine($"Importing texture {assetPath}");

        //    using var image = Image.Load<Rgba32>(assetPath);

        //    byte[] pixels = new byte[image.Width * image.Height * 4];
        //    image.CopyPixelDataTo(pixels);

        //    var asset = new TextureAsset
        //    {
        //        Width = image.Width,
        //        Height = image.Height,
        //        Format = TextureFormat.RGBA8,
        //        MipCount = 1,
        //        PixelData = pixels
        //    };

        //    File.WriteAllBytes(
        //        outputPath,
        //        MessagePackSerializer.Serialize(asset)
        //    );
        //}
    }
}
