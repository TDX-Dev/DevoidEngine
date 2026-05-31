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
                Name = "Priceless Summer - Devoid Engine",
                API = GraphicsAPI.DX11,
                Resizable = false,
                VSync = true
            });

            application.AddLayer(new SandboxProgram());

            application.Run();
        }
    }
}
