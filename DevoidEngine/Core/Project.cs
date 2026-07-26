using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                """;
        }
    }

    public class Project
    {
        public string RootPath = string.Empty;
        public string AssetPath = string.Empty;
        public string EngineCachePath = string.Empty;
        public string BuildSetupPath = string.Empty;

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

            if (project.DevoidVersion.Major != Application.ENGINE_MAJOR_VER || project.DevoidVersion.Minor != Application.ENGINE_MINOR_VER)
            {
                Console.WriteLine("Project was created with a different version of devoid");
            }

            Engine.Instance.VirtualFileSystem.Initialize();
            Engine.Instance.AssetDatabase.Initialize();

        }

        public void Unload()
        {
            Engine.Instance.AssetDatabase.SaveDatabase();
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
