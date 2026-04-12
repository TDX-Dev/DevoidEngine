using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public class SceneImporter : AssetImporter<SceneImportSettings>
    {
        public override string Name => "SceneImporter";

        public override IReadOnlyList<string> Extensions => new[] { ".scene" };

        public override string OutputExtension => "scene";

        public override SceneImportSettings DefaultSettings()
        {
            return new SceneImportSettings();
        }

        public override void Import(
            string assetPath,
            Guid guid,
            SceneImportSettings settings,
            string outputPath)
        {
            byte[] data = File.ReadAllBytes(assetPath);

            File.WriteAllBytes(outputPath, data);
        }
    }
}
