using DevoidEngine.AssetPipeline.Importers;
using DevoidEngine.AssetPipeline.Loaders;
using DevoidEngine.Assets;
using DevoidEngine.Audio;
using DevoidEngine.Core;
using DevoidEngine.UI.Text;
using System.Text.Json;

namespace DevoidEngine.AssetPipeline
{
    public class AssetDatabase
    {
        public string DatabasePath = string.Empty;

        private Dictionary<Guid, AssetEntry> guidToAsset = [];
        private readonly Dictionary<string, AssetEntry> pathToAsset = [];

        public bool TryGetGuid(string assetPath, out Guid guid)
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

        public bool TryGetPath(Guid guid, out string path)
        {
            if (guidToAsset.TryGetValue(guid, out var entry))
            {
                path = entry.AssetPath;
                return true;
            }

            path = "";
            return false;
        }
        public bool TryGetEntry(Guid guid, out AssetEntry? entry)
        {
            if (guidToAsset.TryGetValue(guid, out var assetEntry))
            {
                entry = assetEntry;
                return true;
            }

            entry = default;
            return false;
        }

        void LoadDatabase()
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

                //foreach (var entry in guidToAsset.Values)
                //{
                //    entry.AssetPath = NormalizePath(entry.AssetPath);
                //    //pathToAsset[entry.AssetPath] = entry;
                //    pathToAsset[NormalizePath(entry.AssetPath)] = entry;
                //}
                foreach (var entry in guidToAsset.Values)
                {
                    entry.AssetPath = NormalizePath(entry.AssetPath);

                    // only root assets go in path map
                    if (entry.ContainerGuid == null)
                        pathToAsset[entry.AssetPath] = entry;
                }
            }
            catch
            {
                Console.WriteLine("[AssetDatabase] Failed to load database, rebuilding.");
                guidToAsset.Clear();
                pathToAsset.Clear();
            }
        }

        public void SaveDatabase()
        {
            var state = new AssetDatabaseState
            {
                Entries = guidToAsset
            };

            var data = MessagePack.MessagePackSerializer.Serialize(state);

            File.WriteAllBytes(DatabasePath, data);
        }

        public void RefreshDatabase()
        {
            LoadDatabase();
            ScanAssets();
            SaveDatabase();
        }

        public void Initialize()
        {
            DatabasePath = Path.Combine(Engine.Instance.ProjectSystem.EngineCachePath, "AssetDatabase.bin");
            ImporterRegistry.Register<Texture>(new TextureImporter());
            ImporterRegistry.Register<AudioClip>(new AudioImporter());
            ImporterRegistry.Register<Font>(new FontImporter());
            ImporterRegistry.Register<Scene>(new SceneImporter());
            ImporterRegistry.Register<PackedScene>(new ModelImporter());

            AssetLoaderRegistry.Register<Texture>(new TextureLoader());
            AssetLoaderRegistry.Register<AudioClip>(new AudioLoader());
            AssetLoaderRegistry.Register<Font>(new FontLoader());
            AssetLoaderRegistry.Register<PackedScene>(new PackedSceneLoader());
            AssetLoaderRegistry.Register<Mesh>(new MeshLoader());
            AssetLoaderRegistry.Register<Material>(new MaterialLoader());
            AssetLoaderRegistry.Register<Scene>(new SceneLoader());

            RefreshDatabase();

            //DisplayDatabase();
        }

        public void ScanAssets()
        {
            var assetRoot = Engine.Instance.ProjectSystem.AssetPath;

            CleanupMeta();

            HashSet<Guid> discoveredFiles = [];

            foreach (var file in Directory.GetFiles(assetRoot, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta"))
                    continue;

                string relativePath = NormalizePath(Path.GetRelativePath(assetRoot, file));

                string extension = Path.GetExtension(relativePath).ToLowerInvariant();

                if (!ImporterRegistry.HasImporter(extension))
                    continue;

                Guid guid = RegisterAssetMetaOnly(relativePath);
                discoveredFiles.Add(guid);
            }

            var ordered = discoveredFiles
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
            CleanupLibrary(discoveredFiles);
        }

        internal void ImportAsset(AssetEntry entry)
        {
            var project = Engine.Instance.ProjectSystem;
            string assetPath = entry.AssetPath;

            string absolutePath = NormalizePath(Path.Combine(project.AssetPath, assetPath));

            var meta = LoadMeta(
                Path.Combine(project.AssetPath, entry.MetaPath),
                assetPath
            );

            var importer = ImporterRegistry.GetImporter(
                Path.GetExtension(assetPath).ToLower()
            );

            if (!NeedsReimport(assetPath, meta, entry.Guid))
                return;

            ImportContext context = new()
            {
                AssetPath = absolutePath,
                OutputExtension = importer.OutputExtension,
                OutputDirectory = project.EngineCachePath,
                Guid = entry.Guid,
            };

            importer.Import(context, meta.Settings);

            meta.SourceTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks;

            SaveMeta(Path.Combine(project.AssetPath, entry.MetaPath), meta);
        }

        public Guid RegisterAssetMetaOnly(string assetPath)
        {
            assetPath = NormalizePath(assetPath);
            var metaPath = assetPath + ".meta";


            var absolutePath = Path.Combine(Engine.Instance.ProjectSystem.AssetPath, assetPath);
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
        public Guid RegisterSubAsset(
            Guid container,
            ulong localId)
        {
            Guid guid = CreateSubAssetGuid(container, localId);

            if (!guidToAsset.ContainsKey(guid))
            {
                guidToAsset.Add(guid, new AssetEntry
                {
                    Guid = guid,
                    AssetPath = guidToAsset[container].AssetPath,
                    MetaPath = "",
                    ContainerGuid = container,
                    LocalId = localId
                });
            }

            return guid;
        }

        private bool NeedsReimport(string assetPath, AssetMeta meta, Guid guid)
        {
            var project = Engine.Instance.ProjectSystem;

            var absolutePath = Path.Combine(project.AssetPath, assetPath);
            long currentTimestamp = File.GetLastWriteTimeUtc(absolutePath).Ticks;

            if (currentTimestamp != meta.SourceTimestamp)
                return true;

            var importer = ImporterRegistry.GetImporter(Path.GetExtension(assetPath).ToLower());

            ImportContext context = new()
            {
                AssetPath = absolutePath,
                OutputDirectory = project.EngineCachePath,
                Guid = guid,
                OutputExtension = importer.OutputExtension,
            };

            if (!importer.Exists(context))
                return true;

            return false;
        }

        public void Reimport(Guid guid, byte[]? settings = null)
        {
            if (!guidToAsset.TryGetValue(guid, out var entry))
            {
                Console.WriteLine($"[Asset] Cannot reimport unknown GUID {guid}");
                return;
            }

            if (entry.ContainerGuid != null)
            {
                guid = entry.ContainerGuid.Value;
                entry = guidToAsset[guid];
            }

            var project = Engine.Instance.ProjectSystem;
            string assetPath = entry.AssetPath;

            string absolutePath = NormalizePath(Path.Combine(project.AssetPath, assetPath));

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

            ImportContext context = new()
            {
                AssetPath = absolutePath,
                OutputExtension = importer.OutputExtension,
                OutputDirectory = project.EngineCachePath,
                Guid = entry.Guid,
            };

            importer.Import(context, settings ?? meta.Settings);

            if (settings != null)
            {
                meta.Settings = settings;
                SaveMeta(Path.Combine(project.AssetPath, entry.MetaPath), meta);
            }

            Console.WriteLine($"[Asset] Reimported {assetPath}");
        }

        private AssetMeta CreateMeta(string assetPath, string metaPath)
        {
            var ext = Path.GetExtension(assetPath).ToLower();
            var importer = ImporterRegistry.GetImporter(ext);
            var absolutePath = Path.Combine(Engine.Instance.ProjectSystem.AssetPath, assetPath);

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

        public AssetMeta GetMeta(Guid guid)
        {
            if (!guidToAsset.TryGetValue(guid, out var entry))
                throw new Exception("Asset entry not found");

            string metaPath = Path.Combine(
                Engine.Instance.ProjectSystem.AssetPath,
                entry.MetaPath
            );

            return LoadMeta(metaPath, entry.AssetPath);
        }
        private AssetMeta LoadMeta(string metaPath, string assetPath)
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

        private void SaveMeta(string metaPath, AssetMeta meta)
        {
            var json = JsonSerializer.Serialize(
                meta,
                AssetJsonContext.Default.AssetMeta
            );

            var temp = metaPath + ".tmp";

            File.WriteAllText(temp, json);

            File.Move(temp, metaPath, true);
        }

        public void UpdateMeta(Guid guid, AssetMeta meta)
        {
            if (!guidToAsset.TryGetValue(guid, out var entry))
                throw new Exception("Asset entry not found");

            string metaPath = Path.Combine(
                Engine.Instance.ProjectSystem.AssetPath,
                entry.MetaPath
            );

            SaveMeta(metaPath, meta);
        }

        private bool ValidateMeta(AssetMeta meta)
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


        public string GetLibraryPath(Guid guid, string extension)
        {
            AssetEntry entry = guidToAsset[guid];

            if (entry.ContainerGuid == null)
            {
                return $"{guid:N}.{extension}";
            }

            return $"{entry.ContainerGuid:N}-{entry.LocalId}.{extension}";
        }

        public string GetAssetPath(Guid guid)
        {
            return guidToAsset[guid].AssetPath;
        }
        public IEnumerable<AssetEntry> GetSubAssets(Guid containerGuid)
        {
            foreach (var entry in guidToAsset.Values)
            {
                if (entry.ContainerGuid == containerGuid)
                    yield return entry;
            }
        }
        public AssetEntry GetAssetEntry(Guid guid)
        {
            return guidToAsset[guid];
        }

        private void CleanupMeta()
        {
            var assetRoot = Engine.Instance.ProjectSystem.AssetPath;

            foreach (var meta in Directory.GetFiles(assetRoot, "*.meta", SearchOption.AllDirectories))
            {
                var asset = meta[..^5];

                if (!File.Exists(asset))
                {
                    Console.WriteLine($"Deleting orphan meta: {meta}");
                    File.Delete(meta);
                }
            }
        }

        private void CleanupLibrary(HashSet<Guid> validContainers)
        {
            string library =
                Engine.Instance.ProjectSystem.EngineCachePath;

            foreach (string file in Directory.GetFiles(library))
            {
                if (!TryGetContainerGuid(file, out Guid container))
                    continue;

                if (!validContainers.Contains(container))
                {
                    Console.WriteLine($"Deleting {file}");
                    File.Delete(file);
                }
            }
        }
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            path = path.Replace('\\', '/');

            if (path.StartsWith("./"))
                path = path[2..];

            return path;
        }

        private void DisplayDatabase()
        {
            Console.WriteLine("Asset Database");
            foreach (var kvp in pathToAsset)
            {
                Console.WriteLine($"{kvp.Key} => {kvp.Value.Guid}");
            }
        }

        private static Guid CreateSubAssetGuid(
            Guid container,
            ulong localId)
        {
            Span<byte> buffer = stackalloc byte[24];

            container.TryWriteBytes(buffer);

            BitConverter.TryWriteBytes(
                buffer[16..],
                localId);

            Span<byte> hash = stackalloc byte[16];

            System.Security.Cryptography.MD5.HashData(
                buffer,
                hash);

            return new Guid(hash);
        }

        private static bool TryGetContainerGuid(
    string file,
    out Guid guid)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            int dash = name.IndexOf('-');

            if (dash != -1)
                name = name[..dash];

            return Guid.TryParse(name, out guid);
        }
    }
}
