using DevoidEngine.Engine.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public static class AssetDatabase
    {
        private static Dictionary<Guid, AssetEntry> guidToAsset = new();
        private static Dictionary<string, AssetEntry> pathToAsset = new();

        public static Guid GetGuid(string assetPath)
        {
            return pathToAsset[assetPath].Guid;
        }

        public static string GetPath(Guid guid)
        {
            return guidToAsset[guid].AssetPath;
        }

        public static void Initialize()
        {
            ScanAssets();
        }

        private static void RegisterAsset(string assetPath)
        {
            var metaPath = assetPath + ".meta";

            AssetMeta meta;

            if (!File.Exists(metaPath))
            {
                meta = CreateMeta(assetPath, metaPath);
            }
            else
            {
                meta = LoadMeta(metaPath);
            }

            var entry = new AssetEntry
            {
                Guid = Guid.Parse(meta.Guid),
                AssetPath = assetPath,
                MetaPath = metaPath
            };

            guidToAsset[entry.Guid] = entry;
            pathToAsset[assetPath] = entry;
        }

        private static AssetMeta CreateMeta(string assetPath, string metaPath)
        {
            var meta = new AssetMeta
            {
                Guid = Guid.NewGuid().ToString("N"),
                Importer = GuessImporter(assetPath)
            };

            SaveMeta(metaPath, meta);

            return meta;
        }
        private static AssetMeta LoadMeta(string metaPath)
        {
            var json = File.ReadAllText(metaPath);

            return JsonSerializer.Deserialize(
                json,
                AssetJsonContext.Default.AssetMeta
            );
        }

        private static string GuessImporter(string assetPath)
        {
            var ext = Path.GetExtension(assetPath).ToLower();

            return ext switch
            {
                ".png" => "TextureImporter",
                ".jpg" => "TextureImporter",
                ".fbx" => "ModelImporter",
                ".gltf" => "ModelImporter",
                ".wav" => "AudioImporter",
                ".mp3" => "AudioImporter",
                _ => "DefaultImporter"
            };
        }

        private static void SaveMeta(string metaPath, AssetMeta meta)
        {
            var json = JsonSerializer.Serialize(
                meta,
                AssetJsonContext.Default.AssetMeta
            );

            File.WriteAllText(metaPath, json);
        }

        private static void ScanAssets()
        {
            var assetRoot = ProjectManager.Current.AssetPath;

            foreach (var file in Directory.GetFiles(assetRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta"))
                    continue;

                RegisterAsset(file);
            }
        }
    }
}
