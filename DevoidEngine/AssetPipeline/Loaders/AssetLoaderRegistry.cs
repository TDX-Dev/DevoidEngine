namespace DevoidEngine.AssetPipeline.Loaders
{
    public static class AssetLoaderRegistry
    {
        private static readonly Dictionary<Type, object> loaders = [];
        private static readonly Dictionary<Type, string> runtimeExtensions = [];

        public static bool HasLoader(Type type)
        {
            return loaders.ContainsKey(type);
        }

        public static void Register<T>(IAssetLoader<T> loader)
        {
            loaders[typeof(T)] = loader;
            runtimeExtensions[typeof(T)] = loader.RuntimeExtension;
        }

        public static string? GetRuntimeExtension<T>()
        {
            if (runtimeExtensions.TryGetValue(typeof(T), out var ext))
                return ext;
            return null;
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
