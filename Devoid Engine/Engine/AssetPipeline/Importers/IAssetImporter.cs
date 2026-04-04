using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public interface IAssetImporter
    {
        string Name { get; }

        IReadOnlyList<string> Extensions { get; }
        string OutputExtension { get; }

        Type SettingsType { get; }
        int SettingsVersion { get; }

        byte[] CreateDefaultSettings();

        void Import(string assetPath, Guid guid, byte[] settingsData, string outputPath);
    }
}
