using BepuUtilities.Collections;
using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using MessagePack;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            byte[] pixels;

            switch (settings.Format)
            {
                case TextureFormat.RGBA8_UNorm:
                    {
                        pixels = new byte[width * height * 4];

                        for (int i = 0; i < width * height; i++)
                        {
                            float r = data[i * 4 + 0];
                            float g = data[i * 4 + 1];
                            float b = data[i * 4 + 2];
                            float a = data[i * 4 + 3];

                            if (settings.SRGB)
                            {
                                r = Helper.SRGBToLinear(r);
                                g = Helper.SRGBToLinear(g);
                                b = Helper.SRGBToLinear(b);
                            }

                            pixels[i * 4 + 0] = (byte)(Math.Clamp(r, 0f, 1f) * 255f);
                            pixels[i * 4 + 1] = (byte)(Math.Clamp(g, 0f, 1f) * 255f);
                            pixels[i * 4 + 2] = (byte)(Math.Clamp(b, 0f, 1f) * 255f);
                            pixels[i * 4 + 3] = (byte)(Math.Clamp(a, 0f, 1f) * 255f);
                        }

                        break;
                    }

                case TextureFormat.RGBA16_Float:
                    {
                        Console.WriteLine(new StackTrace(true));
                        pixels = new byte[width * height * 4 * sizeof(ushort)];

                        for (int i = 0; i < data.Length; i++)
                        {
                            ushort bits = (ushort)BitConverter.HalfToUInt16Bits((Half)data[i]);

                            pixels[i * 2 + 0] = (byte)(bits & 0xFF);
                            pixels[i * 2 + 1] = (byte)(bits >> 8);
                        }

                        break;
                    }

                case TextureFormat.RGBA32_Float:
                    {
                        pixels = new byte[data.Length * sizeof(float)];
                        Buffer.BlockCopy(data, 0, pixels, 0, pixels.Length);
                        break;
                    }

                default:
                    throw new NotSupportedException($"Unsupported texture format {settings.Format}");
            }

            var asset = new TextureAsset
            {
                Width = width,
                Height = height,
                Format = settings.Format,
                Filter = settings.Filter,
                Wrap = settings.Wrap,
                Anisotropy = settings.Anisotropy,
                PixelData = pixels,
                GenerateMipmaps = settings.GenerateMipmaps,
            };

            File.WriteAllBytes(
                outputPath,
                MessagePackSerializer.Serialize(asset)
            );
        }
    }
}
