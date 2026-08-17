using System.Text.Json;

namespace DevoidEngine.Core
{
    public class ProjectData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public Version DevoidVersion { get; set; } = null!;
        public string AssetPath { get; set; } = string.Empty;
        public string EngineCachePath { get; set; } = string.Empty;
        public string BuildSetupPath { get; set; } = string.Empty;
        public string ConfigPath { get; set; } = string.Empty;

        public override string ToString()
        {
            return
                $"""
                    Title: {Title}
                    Description: {Description}
                    Created On: {CreatedOn:yyyy-MM-dd HH:mm:ss}
                    Devoid Version: {DevoidVersion}
                    Asset Path: {AssetPath}
                    Engine Cache Path: {EngineCachePath}
                    Build Setup Path: {BuildSetupPath}
                    Config Path: {ConfigPath}
                """;
        }
    }

    public class Project
    {
        public string RootPath = string.Empty;
        public string AssetPath = string.Empty;
        public string EngineCachePath = string.Empty;
        public string BuildSetupPath = string.Empty;
        public string ConfigPath = string.Empty;

        public string SettingsPath => Path.Combine(ConfigPath, "project.settings.json");

        public ProjectSettings Settings { get; private set; } = new();

        public void Load(string path)
        {
            using FileStream stream = File.OpenRead(path);

            ProjectData? project = JsonSerializer.Deserialize(
                stream,
                ProjectJsonContext.Default.ProjectData) ?? throw new Exception("Project file does not exist or is corrupted");

            RootPath = Path.GetDirectoryName(path)!;
            AssetPath = Path.Combine(RootPath, project.AssetPath);
            EngineCachePath = Path.Combine(RootPath, project.EngineCachePath);
            BuildSetupPath = Path.Combine(RootPath, project.BuildSetupPath);
            ConfigPath = Path.Combine(RootPath, project.ConfigPath);

            if (project.DevoidVersion.Major != Application.ENGINE_MAJOR_VER || project.DevoidVersion.Minor != Application.ENGINE_MINOR_VER)
            {
                Console.WriteLine("Project was created with a different version of devoid");
            }

            LoadSettings();

            Engine.Instance.VirtualFileSystem.Initialize();
            Engine.Instance.AssetDatabase.Initialize();

        }

        public void Unload()
        {
            Engine.Instance.AssetDatabase.SaveDatabase();
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                Settings = new ProjectSettings();
                SaveSettings();
                return;
            }

            using FileStream stream = File.OpenRead(SettingsPath);

            Settings = JsonSerializer.Deserialize(
                stream,
                ProjectJsonContext.Default.ProjectSettings)
                ?? new ProjectSettings();
        }

        public void SaveSettings()
        {
            using FileStream stream = File.Create(SettingsPath);

            JsonSerializer.Serialize(
                stream,
                Settings,
                ProjectJsonContext.Default.ProjectSettings);
        }

        public void Create(
            ProjectData data,
            string path
        )
        {
            using FileStream stream = File.Create(path);

            JsonSerializer.Serialize(
                stream,
                data,
                ProjectJsonContext.Default.ProjectData);
        }
    }
}
