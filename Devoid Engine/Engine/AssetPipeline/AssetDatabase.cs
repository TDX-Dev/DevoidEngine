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
        static string DatabasePath => Path.Combine(ProjectManager.Current!.CachePath, "AssetDatabase.bin");

        private static Dictionary<Guid, AssetEntry> guidToAsset = new();
        private static Dictionary<string, AssetEntry> pathToAsset = new();

        public static bool TryGetGuid(string assetPath, out Guid guid)
        {
            assetPath = NormalizePath(assetPath);

            if (pathToAsset.TryGetValue(assetPath, out var entry))
            {
                guid = entry.Guid;
                return true;
            }

            guid = default;
            return false;
        }

        public static bool TryGetPath(Guid guid, out string path)
        {
            if (guidToAsset.TryGetValue(guid, out var entry))
            {
                path = entry.AssetPath;
                return true;
            }

            path = "";
            return false;
        }

        internal static bool TryGetEntry(Guid guid, out AssetEntry? entry)
        {
            if (guidToAsset.TryGetValue(guid, out var assetEntry))
            {
                entry = assetEntry;
                return true;
            }

            entry = default;
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
        public static string GetProjectPath(string path)
        {
            if (ProjectManager.Current == null)
                throw new Exception("Project has not been initialized, tried to use AssetDatabase.");

            return NormalizePath(Path.GetRelativePath(ProjectManager.Current.AssetPath, path));
        }

        public static void Initialize()
        {
            if (ProjectManager.Current == null)
                throw new Exception("Project not initialized");

            ImporterRegistry.Register<Texture2D>(new TextureImporter());
            ImporterRegistry.Register<AudioClip>(new AudioImporter());
            ImporterRegistry.Register<Scene>(new SceneImporter());
            ImporterRegistry.Register<Model>(new ModelImporter());

            RefreshDatabase();
        }

        public static void RefreshDatabase()
        {
            LoadDatabase();
            ScanAssets();
            SaveDatabase();
        }

        static void LoadDatabase()
        {
            if (!File.Exists(DatabasePath))
                return;

            try
            {
                var data = File.ReadAllBytes(DatabasePath);

                var state = MessagePack.MessagePackSerializer
                    .Deserialize<AssetDatabaseState>(data);

                guidToAsset = state.Entries;

                pathToAsset.Clear();

                foreach (var entry in guidToAsset.Values)
                {
                    entry.AssetPath = NormalizePath(entry.AssetPath);
                    //pathToAsset[entry.AssetPath] = entry;
                    pathToAsset[NormalizePath(entry.AssetPath)] = entry;
                }
            }
            catch
            {
                Console.WriteLine("[AssetDatabase] Failed to load database, rebuilding.");
                guidToAsset.Clear();
                pathToAsset.Clear();
            }
        }

        static void SaveDatabase()
        {
            var state = new AssetDatabaseState
            {
                Entries = guidToAsset
            };

            var data = MessagePack.MessagePackSerializer.Serialize(state);

            File.WriteAllBytes(DatabasePath, data);
        }

        internal static Guid RegisterAssetMetaOnly(string assetPath)
        {
            assetPath = NormalizePath(assetPath);

            var metaPath = assetPath + ".meta";

            var absolutePath = Path.Combine(ProjectManager.Current!.AssetPath, assetPath);
            var metaAbsolutePath = absolutePath + ".meta";

            AssetMeta meta;

            if (!File.Exists(metaAbsolutePath))
            {
                meta = CreateMeta(assetPath, metaAbsolutePath);
            }
            else
            {
                meta = LoadMeta(metaAbsolutePath, assetPath);
            }

            var entry = new AssetEntry
            {
                Guid = Guid.Parse(meta.Guid),
                AssetPath = assetPath,
                MetaPath = metaPath,
                ContainerGuid = null,
                LocalId = 0,
            };

            guidToAsset[entry.Guid] = entry;
            pathToAsset[NormalizePath(assetPath)] = entry;

            return entry.Guid;
        }

        internal static void ImportAsset(AssetEntry entry)
        {
            var project = ProjectManager.Current!;
            string assetPath = entry.AssetPath;

            string absolutePath = Path.Combine(project.AssetPath, assetPath);

            var meta = LoadMeta(
                Path.Combine(project.AssetPath, entry.MetaPath),
                assetPath
            );

            var importer = ImporterRegistry.GetImporter(
                Path.GetExtension(assetPath).ToLower()
            );

            if (!NeedsReimport(assetPath, meta, entry.Guid))
                return;

            string output = Path.Combine(
                project.CachePath,
                GetLibraryPath(entry.Guid, importer.OutputExtension)
            );

            importer.Import(absolutePath, entry.Guid, meta.Settings, output);

            meta.SourceTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks;

            SaveMeta(Path.Combine(project.AssetPath, entry.MetaPath), meta);
        }

        internal static Guid RegisterSubAsset(
            Guid containerGuid,
            ulong localId,
            string assetPath)
        {
            assetPath = NormalizePath(assetPath);

            Guid guid = Guid.NewGuid();

            var entry = new AssetEntry
            {
                Guid = guid,
                AssetPath = assetPath,
                MetaPath = guidToAsset[containerGuid].MetaPath,
                ContainerGuid = containerGuid,
                LocalId = localId
            };

            guidToAsset[guid] = entry;

            return guid;
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

        public static void Reimport(Guid guid)
        {
            if (!guidToAsset.TryGetValue(guid, out var entry))
            {
                Console.WriteLine($"[Asset] Cannot reimport unknown GUID {guid}");
                return;
            }

            var project = ProjectManager.Current!;
            string assetPath = entry.AssetPath;

            string absolutePath = Path.Combine(project.AssetPath, assetPath);

            if (!File.Exists(absolutePath))
            {
                Console.WriteLine($"[Asset] Source asset missing: {assetPath}");
                return;
            }

            var meta = LoadMeta(
                Path.Combine(project.AssetPath, entry.MetaPath),
                assetPath
            );

            var importer = ImporterRegistry.GetImporter(
                Path.GetExtension(assetPath).ToLower()
            );

            string output = Path.Combine(
                project.CachePath,
                GetLibraryPath(guid, importer.OutputExtension)
            );

            importer.Import(
                absolutePath,
                guid,
                meta.Settings,
                output
            );

            Console.WriteLine($"[Asset] Reimported {assetPath}");
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

                string relative = NormalizePath(Path.GetRelativePath(assetRoot, file));

                var ext = Path.GetExtension(relative).ToLower();

                if (!ImporterRegistry.HasImporter(ext))
                    continue;

                Guid guid = RegisterAssetMetaOnly(relative);
                discovered.Add(guid);
            }

            // SECOND PASS
            //foreach (var guid in discovered)
            //{
            //    ImportAsset(guidToAsset[guid]);
            //}

            var ordered = discovered
                .Select(g => guidToAsset[g])
                .OrderBy(e =>
                {
                    var importer = ImporterRegistry.GetImporter(
                        Path.GetExtension(e.AssetPath).ToLower()
                    );

                    return importer.Priority;
                });

            foreach (var entry in ordered)
            {
                ImportAsset(entry);
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

        public static AssetMeta GetMeta(Guid guid)
        {
            if (!guidToAsset.TryGetValue(guid, out var entry))
                throw new Exception("Asset entry not found");

            string metaPath = Path.Combine(
                ProjectManager.Current!.AssetPath,
                entry.MetaPath
            );

            return LoadMeta(metaPath, entry.AssetPath);
        }

        public static void UpdateMeta(Guid guid, AssetMeta meta)
        {
            if (!guidToAsset.TryGetValue(guid, out var entry))
                throw new Exception("Asset entry not found");

            string metaPath = Path.Combine(
                ProjectManager.Current!.AssetPath,
                entry.MetaPath
            );

            SaveMeta(metaPath, meta);
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            path = path.Replace('\\', '/');

            if (path.StartsWith("./"))
                path = path.Substring(2);

            return path;
        }
    }
}
