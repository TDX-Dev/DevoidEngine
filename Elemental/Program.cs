using DevoidEngine.Core;
using DevoidEngine.Logging;
using Elemental.Tools.EditorConfiguration;

namespace Elemental
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DevoidLog.AddSink(new ConsoleLogSink());
            DevoidLog.Info(LogCategory.Editor, "Initializing Engine");

            EditorConfig config = EditorConfig.Load();
            SanitizeEditorConfig(ref config);

            ApplicationSpecification engineDescription = new()
            {
                API = config.GraphicsAPI ?? GraphicsAPI.DX11,
                Width = config.Width ?? 1280,
                Height = config.Height ?? 720,
                Name = "Elemental",
                Resizable = config.Resizable ?? true,
                VSync = config.VSync ?? true,
            };

            Application devoidApplication = new(engineDescription);
            
            DevoidLog.Info(LogCategory.Editor, "Initialized Engine");

            DevoidLog.Info(LogCategory.Editor, "Launching Editor");
            devoidApplication.AddLayer(new EditorLayer());

            devoidApplication.Run();
        }

        static void SanitizeEditorConfig(ref EditorConfig config)
        {
            
        }
    }
}
