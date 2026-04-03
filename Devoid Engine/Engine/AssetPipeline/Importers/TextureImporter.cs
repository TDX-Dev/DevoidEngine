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

        public override TextureImportSettings DefaultSettings()
        {
            return new TextureImportSettings();
        }

        public override void Import(
            string assetPath,
            Guid guid,
            TextureImportSettings settings,
            string outputPath
        )
        {
            Console.WriteLine($"Importing texture {assetPath}");

            var bytes = File.ReadAllBytes(assetPath);

            File.WriteAllBytes(outputPath, bytes);
        }
    }
}
