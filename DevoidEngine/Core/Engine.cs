using DevoidEngine.AssetPipeline;
using DevoidEngine.Audio;
using DevoidEngine.Audio.SoLoud;
using DevoidEngine.InputSystem;
using DevoidEngine.Physics;
using DevoidEngine.Physics.Bepu;
using DevoidEngine.Profiling;
using DevoidEngine.Rendering;
using DevoidEngine.UI;
using DevoidGPU;
using DevoidGPU.DX11;

namespace DevoidEngine.Core
{
    public enum GraphicsAPI
    {
        DX11
    }
    public struct EngineConfig
    {
        public GraphicsAPI API;
        public RendererConfig RendererConfig;
        public Version EngineVersion;
    }

    public sealed class Engine
    {
        private static Engine? instance;
        public static Engine Instance => instance ??
            throw new InvalidOperationException("Engine not Initialized.");

        public static Profiler Profiler => Instance.profiler;
        public static IGraphicsDevice GraphicsDevice => Instance.graphicsDevice;
        public static Input InputSystem => Instance.inputSystem;
        public static Renderer Renderer => Instance.renderer;
        public static Cursor Cursor => Instance.cursor;
        public static PhysicsSystem PhysicsSystem => Instance.physicsSystem;
        public static AudioManager AudioSystem => Instance.audioSystem;
        public static UISystem UISystem => Instance.uiSystem;

        public float InterpolationAlpha { get; set; } = 0;
        public float TargetFramerate { get; } = 60f;
        public uint FrameCount { get; internal set; } = 0;
        public float TimeScale { get; set; } = 1f;
        public bool SimulatePhysics { get; set; } = true;
        public bool UseInterpolation { get; set; } = true;
        public SceneTree SceneTree { get; set; } = null!;
        public VirtualFileSystem VirtualFileSystem { get; set; } = null!;
        public Version EngineVersion { get; set; } = null!;
        public Project ProjectSystem { get; set; } = null!;
        public AssetDatabase AssetDatabase { get; set; } = null!;
        public AssetManager AssetManager { get; set; } = null!;

        private readonly Profiler profiler;
        private readonly IGraphicsDevice graphicsDevice;
        private readonly Renderer renderer;
        private readonly Cursor cursor;

        private Input inputSystem = null!;
        private PhysicsSystem physicsSystem = null!;
        private AudioManager audioSystem = null!;
        private UISystem uiSystem = null!;

        private Engine(EngineConfig config)
        {
            EngineVersion = config.EngineVersion;

            profiler = new Profiler();

            graphicsDevice = config.API switch
            {
                GraphicsAPI.DX11 => new DX11GraphicsDevice(),
                _ => throw new ArgumentException("Invalid Graphics API type."),
            };

            renderer = new Renderer();
            cursor = new Cursor();

            ProjectSystem = new Project();
            AssetDatabase = new AssetDatabase();
            AssetManager = new AssetManager();
        }

        public static void Initialize(EngineConfig config)
        {
            if (instance != null)
                throw new InvalidOperationException("Engine already initialized");

            instance = new Engine(config);


            instance.renderer.Initialize(config.RendererConfig);
            instance.SceneTree = new SceneTree();
            instance.inputSystem = new Input();
            instance.physicsSystem = new PhysicsSystem(new BepuPhysicsBackend());
            instance.audioSystem = new AudioManager(new SoLoudAudioBackend());
            instance.uiSystem = new UISystem();

            instance.VirtualFileSystem = new VirtualFileSystem();

            instance.uiSystem.Initialize();

        }
    }
}
