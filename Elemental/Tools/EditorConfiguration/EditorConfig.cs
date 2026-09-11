using DevoidEngine.Core;
using Elemental.Tools.JsonContexts;
using System.Text.Json;

namespace Elemental.Tools.EditorConfiguration
{
    public struct EditorConfig
    {
        public int? Width;
        public int? Height;
        public bool? Resizable;
        public bool? VSync;
        public GraphicsAPI? GraphicsAPI;

        private static string FilePath => Path.Combine(AppContext.BaseDirectory, "editor.json");

        public static EditorConfig Load()
        {
            if (!File.Exists(FilePath))
            {
                EditorConfig config = new()
                {
                    Width = 1280,
                    Height = 720,
                    Resizable = true,
                    VSync = true
                };

                config.Save();
                return config;
            }

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize(json, DevoidJsonContext.Default.EditorConfig);
        }

        public readonly void Save()
        {
            string json = JsonSerializer.Serialize(this, DevoidJsonContext.Default.EditorConfig);
            File.WriteAllText(FilePath, json);
        }
    }
}