using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline
{
    public abstract class AssetImporter<TSettings> : IAssetImporter
    {
        public abstract string Name { get; }
        public abstract IReadOnlyList<string> Extensions { get; }
        public abstract string OutputExtension { get; }

        public Type SettingsType => typeof(TSettings);
        public int SettingsVersion => 0;
        public virtual int Priority => 100;

        public virtual bool IsAssetFolderOnly => false;

        public abstract TSettings DefaultSettings();
        public abstract void Import(
            ImportContext importContext,
            TSettings settings
        );

        byte[] IAssetImporter.CreateDefaultSettings()
        {
            return MessagePackSerializer.Serialize(DefaultSettings());
        }

        void IAssetImporter.Import(ImportContext context, byte[] data)
        {
            TSettings settings;

            try
            {
                settings = MessagePackSerializer.Deserialize<TSettings>(data);
            }
            catch
            {
                Console.WriteLine("Settings incompatible, regenerating defaults.");
                settings = DefaultSettings();
            }

            Import(context, settings);
        }
    }
}
