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
            new[] { ".png", ".jpg", ".jpeg" };

        public override string OutputExtension => "texture";

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

            for (int i = 0; i < data.Length; i++)
            {
                float v = data[i];

                if (v < 0f) v = 0f;
                if (v > 1f) v = 1f;

                pixels[i] = (byte)(v * 255f);
            }

            var asset = new TextureAsset
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA8_UNorm,
                Anisotropy = 8,
                Filter = TextureFilter.Nearest,
                Wrap = TextureWrapMode.ClampToEdge,
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
