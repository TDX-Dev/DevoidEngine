using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    internal class AudioImporter : AssetImporter<AudioImportSettings>
    {
        public override string Name => "AudioImporter";

        public override IReadOnlyList<string> Extensions =>
            new[] { ".mp3", ".wav", ".ogg" };

        public override string OutputExtension => "audio";

        public override AudioImportSettings DefaultSettings()
        {
            return new AudioImportSettings();
        }

        public override void Import(
            string assetPath,
            Guid guid,
            AudioImportSettings settings,
            string outputPath
        )
        {
            Console.WriteLine($"Importing audio {assetPath}");

            var bytes = File.ReadAllBytes(assetPath);

            File.WriteAllBytes(outputPath, bytes);
        }
    }
}
