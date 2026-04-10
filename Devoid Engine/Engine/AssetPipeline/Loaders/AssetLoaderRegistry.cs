using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public static class AssetLoaderRegistry
    {
        private static readonly Dictionary<Type, object> loaders = new();
        public static bool HasLoader(Type type)
        {
            return loaders.ContainsKey(type);
        }

        public static void Register<T>(IAssetLoader<T> loader)
        {
            loaders[typeof(T)] = loader;
        }

        public static bool TryGet<T>(out IAssetLoader<T>? loader)
        {
            if (loaders.TryGetValue(typeof(T), out var value))
            {
                loader = (IAssetLoader<T>)value;
                return true;
            }

            loader = null;
            return false;
        }
    }
}
