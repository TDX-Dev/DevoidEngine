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

        public static void Register<T>(IAssetLoader<T> loader)
        {
            loaders[typeof(T)] = loader;
        }

        public static IAssetLoader<T> Get<T>()
        {
            return (IAssetLoader<T>)loaders[typeof(T)];
        }
    }
}
