using System;
using System.IO;
using System.Xml.Linq;

namespace ElementalEditor
{
    public enum ProjectOperation
    {
        None,
        Load,
        Create
    }

    public class EditorConfig
    {
        public ProjectOperation Operation;
        public string LoadPath;
        public string CreatePath;

        public static EditorConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new Exception("Config file not found: " + path);

            XDocument doc = XDocument.Load(path);

            XElement project = doc.Root.Element("Project");

            if (project == null)
                throw new Exception("Missing <Project> node");

            EditorConfig config = new EditorConfig();

            string op = project.Attribute("Operation")?.Value;

            if (!Enum.TryParse(op, true, out config.Operation))
                throw new Exception("Invalid Operation in config");

            config.LoadPath = project.Element("LoadPath")?.Value;
            config.CreatePath = project.Element("CreatePath")?.Value;

            return config;
        }
    }
}