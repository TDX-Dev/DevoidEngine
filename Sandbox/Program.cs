using DevoidEngine.Core;

namespace Sandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var application = new Application(new ApplicationSpecification()
            {
                Height = 480,
                Width = 640,
                Name = "Devoid",
                API = GraphicsAPI.DX11,
                Resizable = false,
                VSync = true
            });

            application.Run();
        }
    }
}
