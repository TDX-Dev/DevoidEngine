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

        public static void Register(IAssetImporter importer)
        {
            foreach (var ext in importer.Extensions)
                extensionMap[ext] = importer;
        }

        public static IAssetImporter GetImporter(string ext)
        {
            return extensionMap[ext];
        }
    }
}
