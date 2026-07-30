using DevoidEngine.Core;

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

        public static T? Load<T>(Guid guid, bool fromCache = true) where T : class
        {
            if (!Engine.Instance.AssetDatabase.TryGetPath(guid, out var path))
            {
                Console.WriteLine($"Asset not found: {path}");
                return null;
            }

            return Engine.Instance.AssetManager.Load<T>(guid, fromCache);
        }

    }
}
