using DevoidEngine.Engine.AssetPipeline.Importers;
using DevoidEngine.Engine.AssetPipeline.Loaders;
using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace DevoidEngine.Engine.AssetPipeline
{
    static class AssetCache<T>
    {
        public static readonly Dictionary<Guid, T> Cache = new();
    }

    public static class AssetManager
    {

        public static void Initialize()
        {
            AssetLoaderRegistry.Register<Texture2D>(new TextureLoader());
            AssetLoaderRegistry.Register<AudioClip>(new AudioLoader());
            AssetLoaderRegistry.Register<Scene>(new SceneLoader());
            AssetLoaderRegistry.Register<Model>(new ModelLoader());
            AssetLoaderRegistry.Register<Material>(new MaterialLoader());
        }

        public static void Invalidate(Guid guid)
        {
            AssetCache<Texture2D>.Cache.Remove(guid);
            AssetCache<AudioClip>.Cache.Remove(guid);
            AssetCache<Model>.Cache.Remove(guid);
            AssetCache<Scene>.Cache.Remove(guid);
            AssetCache<Material>.Cache.Remove(guid);
        }



        public static T? Load<T>(Guid guid, bool fromCache = true) where T : class?
        {
            if (fromCache)
                if (AssetCache<T>.Cache.TryGetValue(guid, out var asset))
                    return asset;

            if (!AssetDatabase.TryGetEntry(guid, out AssetEntry? entry))
            {
                Console.WriteLine($"[Asset] Missing asset {guid}");
                return default;
            }

            if (entry != null && entry.ContainerGuid != null)
            {
                T? subAsset = ResolveSubAsset<T>(guid, entry.ContainerGuid.Value, entry.LocalId);
                if (subAsset == null)
                {
                    //Console.WriteLine($"[Asset] Missing subasset {guid}");
                } else
                {
                    //Console.WriteLine($"[Asset] Fetched subasset {guid}");
                }
                    return subAsset;
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

            string path = AssetDatabase.GetLibraryPath(guid, extension);

            if (!VirtualFileSystem.Instance.Exists(path))
            {
                Console.WriteLine($"[Asset] Cache missing for {guid}, reimporting...");
                AssetDatabase.Reimport(guid);
            }

            byte[] data = VirtualFileSystem.Instance.ReadAllBytes(path);

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

        private static T? ResolveSubAsset<T>(Guid guid, Guid containerGuid, ulong localId) where T : class?
        {
            var container = Load<Model>(containerGuid);

            if (container == null)
            {
                Console.WriteLine("[Asset] Container missing");
                return default;
            }

            if (!container.TryGetSubAsset<T>(localId, out var asset))
            {
                Console.WriteLine("[Asset] Subasset not found");
                return default;
            }

            if (asset is AssetType at)
                at.Guid = guid;

            AssetCache<T>.Cache[guid] = asset;

            return asset;
        }
    }
}
