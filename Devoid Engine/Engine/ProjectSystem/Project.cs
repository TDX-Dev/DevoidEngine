using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ProjectSystem
{
    public class ProjectConfig
    {
        public string Name { get; set; } = "NewProject";
        public string AssetPath { get; set; } = "Assets";
        public string CachePath { get; set; } = "Cache"; // This is equivalent to Library/
        public string TempPath { get; set; } = "Temp";
        public string SettingsPath { get; set; } = "ProjectSettings";
    }

    public class Project
    {
        public string ProjectFile = null!;
        public string RootPath = null!;

        public string AssetPath = null!;
        public string CachePath = null!;
        public string TempPath = null!;
        public string SettingsPath = null!;

        public ProjectConfig Config = null!;
        public ProjectSettings Settings = new();

        private static void EnsureDirectories(Project p)
        {
            Directory.CreateDirectory(p.AssetPath);
            Directory.CreateDirectory(p.CachePath);
            Directory.CreateDirectory(p.TempPath);
            Directory.CreateDirectory(p.SettingsPath);
        }

        public static Project Load(string projectFile)
        {
            projectFile = Path.GetFullPath(projectFile);

            var json = File.ReadAllText(projectFile);

            var config = JsonSerializer.Deserialize(
                json,
                ProjectJsonContext.Default.ProjectConfig
            ) ?? throw new Exception("Invalid project config");

            var root = Path.GetDirectoryName(projectFile)!;

            var project = new Project
            {
                ProjectFile = projectFile,
                RootPath = root,
                AssetPath = Path.Combine(root, config.AssetPath),
                CachePath = Path.Combine(root, config.CachePath),
                TempPath = Path.Combine(root, config.TempPath),
                SettingsPath = Path.Combine(root, config.SettingsPath),
                Config = config
            };

            EnsureDirectories(project);
            ConfigureSettings(project);

            return project;
        }

        public static Project Create(string directory, string name)
        {
            directory = Path.GetFullPath(directory);
            Directory.CreateDirectory(directory);

            var config = new ProjectConfig
            {
                Name = name
            };

            var projectFile = Path.Combine(directory, "Project.devoid");

            var json = JsonSerializer.Serialize(
                config,
                ProjectJsonContext.Default.ProjectConfig
            );

            File.WriteAllText(projectFile, json);

            var project = Load(projectFile);

            return project;
        }

        static void ConfigureSettings(Project project)
        {
            string settingsFile = Path.Combine(project.SettingsPath, "ProjectSettings.json");

            if (!File.Exists(settingsFile))
            {
                SaveSettings(settingsFile, project.Settings);
            }
            else
            {
                var json = File.ReadAllText(settingsFile);
                project.Settings = JsonSerializer.Deserialize(
                    json,
                    ProjectJsonContext.Default.ProjectSettings
                ) ?? new ProjectSettings();
            }
        }

        static void SaveSettings(string path, ProjectSettings settings)
        {

            var json = JsonSerializer.Serialize(
                settings,
                ProjectJsonContext.Default.ProjectSettings
            );

            File.WriteAllText(path, json);
        }
    }
}
