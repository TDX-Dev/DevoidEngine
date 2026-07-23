using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline
{
    public static class Asset
    {
        public static T? Load<T>(string path, bool fromCache = true) where T : class
        {
            if (!Engine.Instance.AssetDatabase.TryGetGuid(path, out var guid))
            {
                Console.WriteLine($"Asset not found: {path}");
                return null;
            }

            return Engine.Instance.AssetManager.Load<T>(guid, fromCache);
        }

    }
}
