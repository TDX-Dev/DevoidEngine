using DevoidEngine.Engine.AssetPipeline.Importers;
using DevoidEngine.Engine.Core;
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

        public static string GetLibraryPath(Guid guid, string extension)
        {
            var library = ProjectManager.Current.LibraryPath;

            return Path.Combine(library, $"{guid:N}.{extension}");
        }

        public static void Initialize()
        {
            ImporterRegistry.Register<Texture2D>(new TextureImporter());

            ScanAssets();
        }

        private static void RegisterAsset(string assetPath)
        {
            var metaPath = assetPath + ".meta";

            AssetMeta meta;

            bool created = false;
            var absolutePath = Path.Combine(ProjectManager.Current.AssetPath, assetPath);

            if (!File.Exists(absolutePath))
            {
                meta = CreateMeta(assetPath, metaPath);
                created = true;
            }
            else
            {
                meta = LoadMeta(metaPath, assetPath);
            }

            var entry = new AssetEntry
            {
                Guid = Guid.Parse(meta.Guid),
                AssetPath = assetPath,
                MetaPath = metaPath
            };

            guidToAsset[entry.Guid] = entry;
            pathToAsset[assetPath] = entry;

            var ext = Path.GetExtension(assetPath).ToLower();
            var importer = ImporterRegistry.GetImporter(ext);

            if (created || NeedsReimport(assetPath, meta))
            {
                var output = GetLibraryPath(entry.Guid, importer.OutputExtension);

                importer.Import(assetPath, entry.Guid, meta.Settings, output);

                meta.SourceTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks;

                SaveMeta(metaPath, meta);
            }
        }

        private static AssetMeta CreateMeta(string assetPath, string metaPath)
        {
            var ext = Path.GetExtension(assetPath).ToLower();
            var importer = ImporterRegistry.GetImporter(ext);

            var meta = new AssetMeta
            {
                Guid = Guid.NewGuid().ToString("N"),
                Importer = importer.Name,
                Settings = importer.CreateDefaultSettings(),
                SourceTimestamp = File.GetLastWriteTimeUtc(assetPath).Ticks
            };

            SaveMeta(metaPath, meta);

            return meta;
        }
        private static bool NeedsReimport(string assetPath, AssetMeta meta)
        {
            long currentTimestamp = File.GetLastWriteTimeUtc(assetPath).Ticks;

            return currentTimestamp != meta.SourceTimestamp;
        }

        private static AssetMeta LoadMeta(string metaPath, string assetPath)
        {
            try
            {
                var json = File.ReadAllText(metaPath);

                if (string.IsNullOrWhiteSpace(json))
                    throw new Exception("Empty meta file");

                var meta = JsonSerializer.Deserialize(
                    json,
                    AssetJsonContext.Default.AssetMeta
                );

                if (!ValidateMeta(meta))
                    throw new Exception("Invalid meta");

                return meta;
            }
            catch
            {
                Console.WriteLine($"Meta corrupted, regenerating: {metaPath}");

                return CreateMeta(assetPath, metaPath);
            }
        }

        private static bool ValidateMeta(AssetMeta meta)
        {
            if (meta == null)
                return false;

            if (string.IsNullOrWhiteSpace(meta.Guid))
                return false;

            if (!Guid.TryParse(meta.Guid, out _))
                return false;

            if (string.IsNullOrWhiteSpace(meta.Importer))
                return false;

            if (meta.Settings == null)
                return false;

            return true;
        }

        private static void SaveMeta(string metaPath, AssetMeta meta)
        {
            var json = JsonSerializer.Serialize(
                meta,
                AssetJsonContext.Default.AssetMeta
            );

            var temp = metaPath + ".tmp";

            File.WriteAllText(temp, json);

            File.Move(temp, metaPath, true);
        }

        private static void ScanAssets()
        {
            var assetRoot = ProjectManager.Current.AssetPath;

            foreach (var file in Directory.GetFiles(assetRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta"))
                    continue;

                string relative = Path.GetRelativePath(assetRoot, file);
                relative = relative.Replace('\\', '/');

                RegisterAsset(relative);
            }
        }
    }
}
