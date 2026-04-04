using DevoidEngine.Engine.AssetPipeline.Importers;
using DevoidEngine.Engine.AssetPipeline.Loaders;
using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    static class AssetCache<T>
    {
        public static readonly Dictionary<Guid, T> Cache = new();
    }

    public static class AssetManager
    {

        public static void Initialize()
        {
            AssetLoaderRegistry.Register<Texture2D>(new TextureLoader());
            AssetLoaderRegistry.Register<AudioClip>(new AudioLoader());
        }

        public static T? Load<T>(Guid guid)
        {
            if (AssetCache<T>.Cache.TryGetValue(guid, out var asset))
                return asset;

            if (!AssetLoaderRegistry.TryGet<T>(out var loader) || loader == null)
            {
                Console.WriteLine($"No loader registered for {typeof(T).Name}");
                return default;
            }

            string? extension = ImporterRegistry.GetRuntimeExtension<T>();
            if (extension == null)
            {
                Console.WriteLine($"No runtime extension registered for {typeof(T).Name} in importer.");
                return default;
            }

            string path = AssetDatabase.GetLibraryPath(guid, extension);

            if (!VirtualFileSystem.Instance.Exists(path))
            {
                Console.WriteLine($"[Asset] Cache missing for {guid}, reimporting...");
                AssetDatabase.Reimport(guid);
            }

            byte[] data = VirtualFileSystem.Instance.ReadAllBytes(path);

            try
            {
                T loaded = loader.Load(data);

                if (loaded is AssetType assetType)
                    assetType.Guid = guid;

                AssetCache<T>.Cache[guid] = loaded;

                return loaded;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Asset load failed {guid}: {e.Message}");
                return default;
            }
        }
    }
}
