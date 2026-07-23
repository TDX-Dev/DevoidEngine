using DevoidEngine.Core;

namespace Sandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var application = new Application(new ApplicationSpecification()
            {
                Height = 720,
                Width = 1280,
                Name = "Devoid Engine",
                API = GraphicsAPI.DX11,
                Resizable = true,
                VSync = true
            });

            application.AddLayer(new SandboxProgram());

            Engine.Instance.ProjectSystem.Load(
                "D:/Devoid Engine/DevoidProject/new_project.devoid"
            );

            application.Run();
        }
    }
}
