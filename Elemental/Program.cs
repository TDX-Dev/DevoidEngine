using DevoidEngine.Core;

namespace Elemental
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var application = new Application(new ApplicationSpecification()
            {
                Height = 720,
                Width = 1280,
                Name = "Elemental Editor",
                API = GraphicsAPI.DX11,
                Resizable = true,
                VSync = false
            });

            application.AddLayer(new EditorLayer());

            Engine.Instance.ProjectSystem.Load(
                "D:/Devoid Engine/DevoidProject/new_project.devoid"
            );

            application.Run();
        }
    }
}
