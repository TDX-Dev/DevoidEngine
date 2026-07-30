using DevoidEngine.AssetPipeline.Loaders;
using DevoidEngine.Assets;
using DevoidEngine.Core;

namespace DevoidEngine.AssetPipeline
{
    static class AssetCache<T>
    {
        public static readonly Dictionary<Guid, T> Cache = [];
    }
    public class AssetManager
    {

        public void Invalidate(Guid guid)
        {

        }

        public T? Load<T>(Guid guid, bool fromCache = true) where T : class?
        {
            if (fromCache)
                if (AssetCache<T>.Cache.TryGetValue(guid, out var asset))
                    return asset;
            if (!Engine.Instance.AssetDatabase.TryGetEntry(guid, out _))
            {
                Console.WriteLine($"[Asset] Missing asset {guid}");
                return default;
            }

            if (!AssetLoaderRegistry.TryGet<T>(out var loader) || loader == null)
            {
                Console.WriteLine($"No loader registered for {typeof(T).Name}");
                return default;
            }

            string? extension = ImporterRegistry.GetRuntimeExtension<T>();
            if (extension == null)
            {
                Console.WriteLine($"No runtime extension registered for {typeof(T).Name} in importer.");
                return default;
            }



            string path;
            if (ImporterRegistry.IsAssetFolderOnly(Path.GetExtension(Engine.Instance.AssetDatabase.GetAssetEntry(guid).AssetPath)))
            {
                path = Engine.Instance.AssetDatabase.GetAssetPath(guid);
            }
            else
            {
                path = Engine.Instance.AssetDatabase.GetLibraryPath(guid, extension);
            }

            if (!Engine.Instance.VirtualFileSystem.Exists(path))
            {
                Console.WriteLine($"[Asset] Cache missing for {guid}, reimporting...");
                Engine.Instance.AssetDatabase.Reimport(guid);
            }

            byte[] data = Engine.Instance.VirtualFileSystem.ReadAllBytes(path);

            try
            {
                T loaded = loader.Load(data);

                if (loaded is AssetType assetType)
                    assetType.Guid = guid;

                AssetCache<T>.Cache[guid] = loaded;

                return loaded;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Asset load failed {guid}: {e.Message}");
                return default;
            }
        }

    }
}
