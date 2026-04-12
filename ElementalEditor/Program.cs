using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using DevoidGPU.DX11;
using ElementalEditor.Scripting;
using System;
using System.IO;

namespace ElementalEditor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string configPath = Path.Combine(Environment.CurrentDirectory, "editor.config");

            EditorConfig config = EditorConfig.Load(configPath);

            string projectFile = null;

            if (config.Operation == ProjectOperation.Load)
            {
                if (string.IsNullOrEmpty(config.LoadPath))
                    throw new Exception("LoadPath missing in config");

                projectFile = config.LoadPath;
            }
            else if (config.Operation == ProjectOperation.Create)
            {
                if (string.IsNullOrEmpty(config.CreatePath))
                    throw new Exception("CreatePath missing in config");

                ProjectManager.Create(config.CreatePath, "NewProject");
                projectFile = Path.Combine(config.CreatePath, "Project.devoid");
            }

            Console.WriteLine("Loading project...");
            ProjectManager.Load(projectFile);

            Console.WriteLine($"Project Loaded: {ProjectManager.Current.Config.Name}");
            Console.WriteLine($"Assets: {ProjectManager.Current.AssetPath}");

            Console.WriteLine("Initializing Script System: ");

            ScriptProjectGenerator.Generate();

            // compile scripts if needed
            ScriptCompiler.Compile(out string errors);

            // load assembly
            ScriptAssemblyLoader.Load();

            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                darkTitlebar = true,
                forceVsync = true,
                graphicsDevice = new DX11GraphicsDevice(),
                Width = 1280,
                Height = 720,
                useFullscreen = false,
                useImGui = true,
                useDebugConsole = true,
                Name = "Devoid Editor",
                allowResize = true
            };

            Application application = new Application();

            VirtualFileSystem.Initialize();
            AssetManager.Initialize();
            AssetDatabase.Initialize();

            application.Initialize(applicationSpecification);

            application.TargetFrameRate = 60;
            EngineSingleton.Instance.UseInterpolation = true;

            application.AddLayer(new EditorLayer());
            application.ApplyProjectSettings();

            application.Run();
        }
    }
}