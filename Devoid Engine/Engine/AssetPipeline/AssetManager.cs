using DevoidEngine.Engine.AssetPipeline.Importers;
using DevoidEngine.Engine.AssetPipeline.Loaders;
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
            AssetLoaderRegistry.Register<Texture>(new TextureLoader());
        }

        public static T Load<T>(Guid guid)
        {
            if (AssetCache<T>.Cache.TryGetValue(guid, out var asset))
                return asset;

            var loader = AssetLoaderRegistry.Get<T>();

            string extension = ImporterRegistry.GetRuntimeExtension<T>();
            string path = AssetDatabase.GetLibraryPath(guid, extension);

            byte[] data = VirtualFileSystem.Instance.ReadAllBytes(path);

            T loaded = loader.Load(data);

            AssetCache<T>.Cache[guid] = loaded;

            return loaded;
        }
    }
}
