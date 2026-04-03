using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public abstract class AssetImporter<TSettings> : IAssetImporter
    {
        public abstract string Name { get; }

        public abstract IReadOnlyList<string> Extensions { get; }
        public abstract string OutputExtension { get; }

        public Type SettingsType => typeof(TSettings);

        public abstract TSettings DefaultSettings();

        public abstract void Import(
            string assetPath,
            Guid guid,
            TSettings settings,
            string outputPath
        );

        byte[] IAssetImporter.CreateDefaultSettings()
        {
            return MessagePackSerializer.Serialize(DefaultSettings());
        }

        void IAssetImporter.Import(string path, Guid guid, byte[] data, string outputPath)
        {
            var settings = MessagePackSerializer.Deserialize<TSettings>(data);

            Import(path, guid, settings, outputPath);
        }
    }
}
