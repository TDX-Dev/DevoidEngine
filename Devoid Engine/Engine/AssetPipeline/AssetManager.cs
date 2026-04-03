using DevoidEngine.Engine.AssetPipeline.Loaders;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public static class AssetManager
    {
        private static readonly Dictionary<Guid, object> cache = new();

        public static void Initialize()
        {
            AssetLoaderRegistry.Register(new TextureLoader());
            AssetLoaderRegistry.Register(new ShaderLoader());
        }

        public static T Load<T>(Guid guid)
        {
            if (cache.TryGetValue(guid, out var asset))
                return (T)asset;

            string extension = AssetExtensions.Get<T>();
            string path = AssetDatabase.GetLibraryPath(guid, extension);

            var vfs = EngineSingleton.Instance.VirtualFileSystem;
            byte[] data = vfs.ReadAllBytes(path);

            var loader = AssetLoaderRegistry.Get<T>();
            T loaded = loader.Load(data);

            cache[guid] = loaded;

            return loaded;
        }
    }
}
