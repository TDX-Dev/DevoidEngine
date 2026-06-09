using DevoidEngine.InputSystem;
using DevoidEngine.Profiling;
using DevoidEngine.Rendering;
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

        public float InterpolationAlpha { get; set; } = 0;
        public float TargetFramerate { get; } = 60f;
        public uint FrameCount { get; internal set; } = 0;
        public float TimeScale { get; set; } = 1.0f;
        public bool SimulatePhysics { get; set; } = true;
        public SceneTree SceneTree { get; set; } = null!;

        private readonly Profiler profiler;
        private readonly IGraphicsDevice graphicsDevice;
        private readonly Renderer renderer;

        private Input inputSystem = null!;

        private Engine(EngineConfig config)
        {
            profiler = new Profiler();

            graphicsDevice = config.API switch
            {
                GraphicsAPI.DX11 => new DX11GraphicsDevice(),
                _ => throw new ArgumentException("Invalid Graphics API type."),
            };

            renderer = new Renderer();
        }

        public static void Initialize(EngineConfig config)
        {
            if (instance != null)
                throw new InvalidOperationException("Engine already initialized");

            instance = new Engine(config);

            instance.renderer.Initialize(config.RendererConfig);
            instance.SceneTree = new SceneTree();
            instance.inputSystem = new Input();
        }
    }
}
