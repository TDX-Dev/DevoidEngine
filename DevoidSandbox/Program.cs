using DevoidEngine.Core;

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
                Width = 1920,
                Height = 1080,
            };

            Application devoidApplication = new(devoidAppSpec);


            devoidApplication.Run();
        }
    }
}
