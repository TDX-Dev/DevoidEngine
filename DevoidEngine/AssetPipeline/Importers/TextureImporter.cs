using DevoidEngine.Assets;
using DevoidEngine.Util;
using DevoidGPU;
using MessagePack;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidEngine.AssetPipeline.Importers
{
    public class TextureImporter : AssetImporter<TextureImportSettings>
    {
        public override string Name => "TextureImporter";

        public override IReadOnlyList<string> Extensions => [".png", ".jpg", ".jpeg", ".hdr"];

        public override string OutputExtension => "texture";

        public override int Priority => 100;

        public override TextureImportSettings DefaultSettings()
        {
            return new TextureImportSettings();
        }

        public override void Import(ImportContext context, TextureImportSettings settings)
        {
            TextureFormat outputFormat = settings.Format;

            if (settings.SRGB && settings.Format == TextureFormat.RGBA8_UNorm)
            {
                outputFormat = TextureFormat.RGBA8_UNorm_SRGB;
            }

            byte[] fileBytes = File.ReadAllBytes(context.AssetPath);

            ImageData image = TextureUtil.LoadImage(fileBytes);

            int width = image.Width;
            int height = image.Height;
            byte[] data = image.Data;

            byte[] pixels;

            switch (outputFormat)
            {
                case TextureFormat.RGBA8_UNorm:
                case TextureFormat.RGBA8_UNorm_SRGB:
                    {
                        pixels = data;
                        break;
                    }

                case TextureFormat.RGBA16_Float:
                    {
                        if (image.Format == TextureFormat.RGBA8_UNorm)
                        {
                            pixels = new byte[width * height * 4 * Unsafe.SizeOf<Half>()];

                            for (int i = 0; i < data.Length; i++)
                            {
                                float value = data[i] / 255.0f;

                                if (settings.SRGB)
                                    value = TextureUtil.SRGBToLinear(value);

                                Half h = (Half)value;
                                ushort bits = (ushort)BitConverter.HalfToUInt16Bits(h);

                                pixels[i * 2 + 0] = (byte)(bits & 0xFF);
                                pixels[i * 2 + 1] = (byte)(bits >> 8);
                            }
                        }
                        else if (image.Format == TextureFormat.RGBA32_Float)
                        {
                            ReadOnlySpan<float> floats =
                                MemoryMarshal.Cast<byte, float>(data);

                            pixels = new byte[floats.Length * Unsafe.SizeOf<Half>()];

                            for (int i = 0; i < floats.Length; i++)
                            {
                                Half h = (Half)floats[i];
                                ushort bits = (ushort)BitConverter.HalfToUInt16Bits(h);

                                pixels[i * 2 + 0] = (byte)(bits & 0xFF);
                                pixels[i * 2 + 1] = (byte)(bits >> 8);
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        break;
                    }

                case TextureFormat.RGBA32_Float:
                    {
                        pixels = data;
                        break;
                    }

                default:
                    throw new NotSupportedException($"Unsupported texture format {settings.Format}");
            }

            var asset = new TextureAsset
            {
                Width = width,
                Height = height,
                Format = outputFormat,
                Filter = settings.Filter,
                Wrap = settings.Wrap,
                Anisotropy = settings.Anisotropy,
                PixelData = pixels,
                GenerateMipmaps = settings.GenerateMipmaps,
            };

            File.WriteAllBytes(
                context.GetRootOutputPath(context.OutputExtension),
                MessagePackSerializer.Serialize(asset)
            );
        }

        public override bool Exists(ImportContext context)
        {
            return File.Exists(context.GetRootOutputPath(OutputExtension));
        }
    }
}
