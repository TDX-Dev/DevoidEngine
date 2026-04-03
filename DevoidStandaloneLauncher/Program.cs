using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using DevoidGPU.DX11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                darkTitlebar = true,
                forceVsync = true,
                graphicsDevice = new DX11GraphicsDevice(),
                Width = 1280,
                Height = 720,
                useFullscreen = true,
                useImGui = true,
                useDebugConsole = true,
                Name = "Devoid New Beginnings"
            };

            LoadProject();
            AssetDatabase.Initialize();

            Application application = new Application();
            application.Initialize(applicationSpecification);
            application.TargetFrameRate = 10;
            EngineSingleton.Instance.UseInterpolation = false;
            application.AddLayer(new PrototypeLoader());


            application.Run();
        }

        static void LoadProject()
        {
            var projectRoot = "D:\\Programming\\Devoid Engine\\DevoidStandaloneLauncher\\Project";
            var projectFile = Path.Combine(projectRoot, "Project.devoid");

            if (!File.Exists(projectFile))
            {
                Console.WriteLine("Creating project...");
                ProjectManager.Create(projectRoot, "DevoidGame");
            }
            else
            {
                Console.WriteLine("Loading project...");
                ProjectManager.Load(projectFile);
            }

            Console.WriteLine($"Project Loaded: {ProjectManager.Current.Config.Name}");
            Console.WriteLine($"Assets: {ProjectManager.Current.AssetPath}");
        }


    }
}
