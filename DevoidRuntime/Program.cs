using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Rendering;
using DevoidGPU.DX11;

namespace DevoidRuntime
{
    class Program
    {
        static string projectFile = null!;
        static string mode = "game";
        static string? sceneOverride = null;

        static void Main(string[] args)
        {
            ParseArguments(args);

            LoadProject();
            ScriptLoader.LoadScripts();

            InitializeVFS();
            InitializeAssets();

            ApplicationSpecification spec = CreateApplicationSpec();

            Application app = new Application();
            app.Initialize(spec);
            app.ApplyProjectSettings();

            DebugRenderSystem.AllowDebugDraw = false;

            app.AddLayer(new RuntimeLayer());
            LoadStartupScene();
            app.Run();
        }

        static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--project")
                    projectFile = args[i + 1];

                if (args[i] == "--mode")
                    mode = args[i + 1];

                if (args[i] == "--scene")
                    sceneOverride = args[i + 1];
            }
        }

        static void LoadProject()
        {
            if (string.IsNullOrEmpty(projectFile))
                throw new Exception("Runtime requires --project argument");

            Console.WriteLine("Loading project...");
            ProjectManager.Load(projectFile);

            Console.WriteLine($"Project Loaded: {ProjectManager.Current.Config.Name}");
            Console.WriteLine($"Assets: {ProjectManager.Current.AssetPath}");
        }

        static void InitializeVFS()
        {
            if (mode == "editor")
            {
                var project = ProjectManager.Current;

                VirtualFileSystem.Initialize(
                    new DirectorySource(project.CachePath),
                    new DirectorySource(project.AssetPath)
                );
            }
            else
            {
                throw new NotImplementedException("Pak Source publishing not implemented yet");

                //string pakPath = Path.Combine(
                //    AppContext.BaseDirectory,
                //    "Game.pak");

                //VirtualFileSystem.Initialize(
                //    new PakSource(pakPath)
                //);
            }
        }

        static void InitializeAssets()
        {
            AssetManager.Initialize();
            AssetDatabase.Initialize();
        }

        static Scene LoadStartupScene()
        {
            var project = ProjectManager.Current;

            string scenePath;

            if (!string.IsNullOrEmpty(sceneOverride))
            {
                scenePath = sceneOverride;
                Console.WriteLine($"Loading scene override: {scenePath}");
            }
            else
            {
                scenePath = project.Settings.StartupScene;

                if (string.IsNullOrEmpty(scenePath))
                    throw new Exception("No startup scene configured");
            }

            var scene = Asset.Load<Scene>(scenePath);
            SceneManager.LoadScene(scene);

            scene.Play(true);

            return scene;
        }


        static ApplicationSpecification CreateApplicationSpec()
        {
            var project = ProjectManager.Current;

            return new ApplicationSpecification()
            {
                Name = project.Config.Name,
                Width = project.Settings.RenderWidth,
                Height = project.Settings.RenderHeight,
                graphicsDevice = new DX11GraphicsDevice(),
                useImGui = false
            };
        }
    }
}