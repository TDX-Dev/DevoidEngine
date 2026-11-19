using DevoidEngine.InputSystem;
using DevoidEngine.Profiling;
using DevoidEngine.Rendering;
using DevoidGPU;
using DevoidGPU.DX11;

namespace DevoidEngine.Core
{
    public enum GraphicsApi
    {
        Dx11
    }
    public struct EngineConfig
    {
        public GraphicsApi Api;
        public RendererConfig RendererConfig;
        public Version EngineVersion;
    }

    public sealed class Engine
    {
        private static Engine? instance;
        public static Engine Instance => instance ??
            throw new InvalidOperationException("Engine not Initialized.");

        public static Profiler Profiler => Instance._profiler;
        public static IGraphicsDevice GraphicsDevice => Instance._graphicsDevice;
        public static Input InputSystem => Instance._inputSystem;
        public static Renderer Renderer => Instance._renderer;
        public static Cursor Cursor => Instance._cursor;
#if DEBUG
        public static string BasePath => BuildInfo.EngineRoot;
#else
        public static string BasePath => AppContext.BaseDirectory;
#endif

        public float InterpolationAlpha { get; set; } = 0;
        public float SimulationTargetFramerate { get; } = 60f;
        public uint FrameCount { get; internal set; } = 0;
        public float TimeScale { get; set; } = 1f;
        public bool SimulatePhysics { get; set; } = true;
        public bool UseInterpolation { get; set; } = true;
        public TextureManager TextureManager { get; internal set; } = null!;
        public ViewportManager ViewportManager { get; internal set; } = null!;
        public VirtualFileSystem VirtualFileSystem { get; set; } = null!;
        public Version EngineVersion { get; set; } = null!;
        public Project ProjectSystem { get; set; } = null!;

        public GraphicsDeviceInfo GraphicsDeviceInfo { get; set; }

        private readonly Profiler _profiler;
        private readonly IGraphicsDevice _graphicsDevice;
        private readonly Renderer _renderer;
        private readonly Cursor _cursor;

        private Input _inputSystem = null!;

        private Engine(EngineConfig config)
        {
            EngineVersion = config.EngineVersion;

            _profiler = new Profiler();

            _graphicsDevice = config.Api switch
            {
                GraphicsApi.Dx11 => new Dx11GraphicsDevice(),
                _ => throw new ArgumentException("Invalid Graphics API type."),
            };

            GraphicsDeviceInfo = _graphicsDevice.Info;

            _renderer = new Renderer();
            _cursor = new Cursor();

            ProjectSystem = new Project();
            TextureManager = new TextureManager();
        }

        public static void Initialize(EngineConfig config)
        {
            if (instance != null)
                throw new InvalidOperationException("Engine already initialized");

            instance = new Engine(config);


            instance._renderer.Initialize(config.RendererConfig);
            instance.ViewportManager = new ViewportManager();
            instance._inputSystem = new Input();

            instance.VirtualFileSystem = new VirtualFileSystem();

        }
    }
}
