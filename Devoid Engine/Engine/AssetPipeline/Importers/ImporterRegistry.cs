using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public static class ImporterRegistry
    {
        private static Dictionary<string, IAssetImporter> extensionMap = new();
        private static Dictionary<Type, string> runtimeExtensions = new();

        public static void Register<TAsset>(IAssetImporter importer)
        {
            foreach (var ext in importer.Extensions)
                extensionMap[ext.ToLower()] = importer;

            runtimeExtensions[typeof(TAsset)] = importer.OutputExtension;
        }

        public static string GetRuntimeExtension<T>()
        {
            return runtimeExtensions[typeof(T)];
        }

        public static bool HasImporter(string ext)
        {
            return extensionMap.ContainsKey(ext);
        }

        public static IAssetImporter GetImporter(string ext)
        {
            return extensionMap[ext];
        }
    }
}
