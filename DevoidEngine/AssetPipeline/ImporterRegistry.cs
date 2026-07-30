namespace DevoidEngine.AssetPipeline
{
    public static class ImporterRegistry
    {
        private static readonly Dictionary<string, IAssetImporter> extensionMap = [];
        private static readonly Dictionary<Type, string> runtimeExtensions = [];

        public static void Register<TAsset>(IAssetImporter importer)
        {
            foreach (var ext in importer.Extensions)
                extensionMap[ext.ToLower()] = importer;

            runtimeExtensions[typeof(TAsset)] = importer.OutputExtension;
        }

        public static string? GetRuntimeExtension<T>()
        {
            if (runtimeExtensions.TryGetValue(typeof(T), out var ext))
                return ext;
            return null;
        }

        public static bool HasImporter(string ext)
        {
            return extensionMap.ContainsKey(ext);
        }

        public static bool IsAssetFolderOnly(string ext)
        {
            IAssetImporter importer = extensionMap[ext.ToLower()];
            return importer.IsAssetFolderOnly;
        }

        public static IAssetImporter GetImporter(string ext)
        {
            return extensionMap[ext];
        }
    }
}
