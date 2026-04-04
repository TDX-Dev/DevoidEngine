using DevoidEngine.Engine.AssetPipeline.Importers;
using DevoidEngine.Engine.AudioSystem;
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

        public static bool TryGetGuid(string assetPath, out Guid guid)
        {
            if (pathToAsset.TryGetValue(assetPath, out var entry))
            {
                guid = entry.Guid;
                return true;
            }

            guid = default;
            return false;
        }

        public static string GetPath(Guid guid)
        {
            return guidToAsset[guid].AssetPath;
        }

        public static string GetLibraryPath(Guid guid, string extension)
        {
            return $"{guid:N}.{extension}";
        }

        public static void Initialize()
        {
            if (ProjectManager.Current == null)
                throw new Exception("Project not initialized");

            ImporterRegistry.Register<Texture2D>(new TextureImporter());
            ImporterRegistry.Register<AudioClip>(new AudioImporter());

            ScanAssets();
        }

        private static Guid RegisterAsset(string assetPath)
        {
            var metaPath = assetPath + ".meta";

            AssetMeta meta;

            bool created = false;
            var absolutePath = Path.Combine(ProjectManager.Current!.AssetPath, assetPath);
            var metaAbsolutePath = absolutePath + ".meta";

            if (!File.Exists(metaAbsolutePath))
            {
                meta = CreateMeta(assetPath, metaAbsolutePath);
                created = true;
            }
            else
            {
                meta = LoadMeta(metaAbsolutePath, assetPath);
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

            if (created || NeedsReimport(assetPath, meta, entry.Guid))
            {
                var output = Path.Combine(
                    ProjectManager.Current.CachePath,
                    GetLibraryPath(entry.Guid, importer.OutputExtension)
                );

                importer.Import(absolutePath, entry.Guid, meta.Settings, output);

                meta.SourceTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks;

                //SaveMeta(metaPath, meta);
                SaveMeta(metaAbsolutePath, meta);
            }
            return entry.Guid;
        }

        private static AssetMeta CreateMeta(string assetPath, string metaPath)
        {
            var ext = Path.GetExtension(assetPath).ToLower();
            var importer = ImporterRegistry.GetImporter(ext);
            var absolutePath = Path.Combine(ProjectManager.Current!.AssetPath, assetPath);

            var meta = new AssetMeta
            {
                Guid = Guid.NewGuid().ToString("N"),
                Importer = importer.Name,
                Settings = importer.CreateDefaultSettings(),
                Version = 1,
                SourceTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks
            };

            SaveMeta(metaPath, meta);

            return meta;
        }
        private static bool NeedsReimport(string assetPath, AssetMeta meta, Guid guid)
        {
            var project = ProjectManager.Current!;

            var absolutePath = Path.Combine(project.AssetPath, assetPath);
            long currentTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks;

            if (currentTimestamp != meta.SourceTimestamp)
                return true;

            var importer = ImporterRegistry.GetImporter(Path.GetExtension(assetPath).ToLower());

            var libraryPath = Path.Combine(
                project.CachePath,
                GetLibraryPath(guid, importer.OutputExtension)
            );

            if (!File.Exists(libraryPath))
                return true;

            return false;
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

                if (!ValidateMeta(meta!))
                    throw new Exception("Invalid meta");

                return meta!;
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
            var assetRoot = ProjectManager.Current!.AssetPath;

            CleanupMeta();

            HashSet<Guid> discovered = new();

            foreach (var file in Directory.GetFiles(assetRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta"))
                    continue;

                string relative = Path.GetRelativePath(assetRoot, file);
                relative = relative.Replace('\\', '/');

                var ext = Path.GetExtension(relative).ToLower();

                if (!ImporterRegistry.HasImporter(ext))
                    continue;

                Guid guid = RegisterAsset(relative);
                discovered.Add(guid);
            }

            CleanupLibrary(discovered);
        }

        private static void CleanupMeta()
        {
            var assetRoot = ProjectManager.Current!.AssetPath;

            foreach (var meta in Directory.GetFiles(assetRoot, "*.meta", SearchOption.AllDirectories))
            {
                var asset = meta.Substring(0, meta.Length - 5);

                if (!File.Exists(asset))
                {
                    Console.WriteLine($"Deleting orphan meta: {meta}");
                    File.Delete(meta);
                }
            }
        }

        private static void CleanupLibrary(HashSet<Guid> validGuids)
        {
            var library = ProjectManager.Current!.CachePath;

            foreach (var file in Directory.GetFiles(library))
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (!Guid.TryParse(name, out var guid))
                    continue;

                if (!validGuids.Contains(guid))
                {
                    Console.WriteLine($"Deleting orphaned asset: {file}");
                    File.Delete(file);
                }
            }
        }
    }
}
