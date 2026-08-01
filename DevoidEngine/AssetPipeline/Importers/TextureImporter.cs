using DevoidEngine.Assets;
using DevoidEngine.Util;
using DevoidGPU;
using MessagePack;

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
            byte[] fileBytes = File.ReadAllBytes(context.AssetPath);

            ImageData image = TextureUtil.LoadImage(fileBytes);

            int width = image.Width;
            int height = image.Height;
            byte[] data = image.Data;

            byte[] pixels;

            switch (settings.Format)
            {
                case TextureFormat.RGBA8_UNorm:
                    {
                        pixels = data;

                        break;
                    }

                case TextureFormat.RGBA16_Float:
                    {
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
                Format = settings.Format,
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
