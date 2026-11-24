using DevoidEngine.Core;
using DevoidEngine.Logging;

namespace DevoidSandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ApplicationSpecification devoidAppSpec = new()
            {
                Name = "Devoid Sandbox Application",
                Api = GraphicsAPI.DX11,
                Resizable = false,
                VSync = true,
                Width = 1920 / 4,
                Height = 1080 / 4,
            };

            DevoidLog.AddSink(new ConsoleLogSink());

            Application devoidApplication = new(devoidAppSpec);

            devoidApplication.AddLayer(new SandboxLayer());

            devoidApplication.Run();
        }
    }
}
