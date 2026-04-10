using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using DevoidGPU.DX11;

namespace DevoidRuntime
{
    class Program
    {
        static void Main(string[] args)
        {
            string projectFile = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--project")
                    projectFile = args[i + 1];
            }

            ProjectManager.Load(projectFile);

            VirtualFileSystem.Initialize();
            AssetManager.Initialize();
            AssetDatabase.Initialize();

            ApplicationSpecification spec = new ApplicationSpecification()
            {
                Name = ProjectManager.Current.Config.Name,
                Width = ProjectManager.Current.Settings.RenderWidth,
                Height = ProjectManager.Current.Settings.RenderHeight,
                graphicsDevice = new DX11GraphicsDevice(),
                useImGui = false
            };

            Application app = new Application();
            app.Initialize(spec);

            //SceneManager.LoadStartupScene();

            app.Run();
        }
    }
}
