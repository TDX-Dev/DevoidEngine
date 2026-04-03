using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class Project
    {
        public string RootPath;
        public string AssetPath;
        public string LibraryPath;
        public string SettingsPath;

        public ProjectSettings Settings;

        public static Project Load(string projectFile)
        {
            var json = File.ReadAllText(projectFile);
            var config = JsonSerializer.Deserialize<ProjectConfig>(json);

            var root = Path.GetDirectoryName(projectFile);

            return new Project
            {
                RootPath = root,
                AssetPath = Path.Combine(root, config.AssetPath),
                LibraryPath = Path.Combine(root, config.LibraryPath),
                SettingsPath = Path.Combine(root, config.SettingsPath)
            };
        }
    }
}
