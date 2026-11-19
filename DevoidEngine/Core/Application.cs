

#define PROFILING
#define DISPLAY_DEBUG_INFO

using DevoidEngine.Imgui;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Core
{
    public struct ApplicationSpecification
    {
        public string Name;

        public int Width;
        public int Height;

        public bool VSync;
        public bool Resizable;

        public GraphicsApi Api;
    }

    public class Application
    {
        public const int EngineMajorVer = 0;
        public const int EngineMinorVer = 1;

        public WindowSurface MainWindow => _mainSurface;
        public readonly ImGuiRenderer ImguiRenderer;

        private readonly WindowSurface _mainSurface;
        private readonly List<WindowSurface> _surfaces;
        private readonly FrameTimer _frameTimer;

        private readonly LayerManager _layerManager;
        private float _deltaTimeAccumulator = 0f;
        private uint _numFrames = 0;
        private bool _isRunning = true;
        private float _systemInfoTimer;

        public Application(ApplicationSpecification specification)
        {
            _surfaces = [];
            _frameTimer = new();

            EngineConfig configuration = new()
            {
                Api = GraphicsApi.Dx11,
                RendererConfig = new RendererConfig()
                {
                    Technique = RenderTechnique.Forward
                },
                EngineVersion = new Version(EngineMajorVer, EngineMinorVer)
            };

            Engine.Initialize(configuration);

            _layerManager = new LayerManager();

            var window = new Window(new WindowSpecification
            {
                Title = specification.Name,
                Width = specification.Width,
                Height = specification.Height,
                Resizable = specification.Resizable,
                StartVisible = false,
                StartCentered = true,
                StartFocused = true,
                Vsync = specification.VSync
            });

            _mainSurface = new WindowSurface(
                window,
                Engine.GraphicsDevice,
                new SwapchainDescription()
                {
                    BufferCount = 2,
                    Format = DevoidGPU.TextureFormat.Rgba8UNorm,
                    Height = specification.Height,
                    Width = specification.Width,
                    RefreshRate = Vector2.Zero,
                    Samples = new DevoidGPU.TextureSampleDescription(1, 0),
                    VSync = specification.VSync,
                    Windowed = true
                }
            );

            _surfaces.Add(_mainSurface);

            Engine.InputSystem.UpdateInputProviderWindow(window);

            ImguiRenderer = new ImGuiRenderer();
            ImguiRenderer.Initialize(_mainSurface);
            ImguiRenderer.OnGui += () => { _layerManager.OnGUILayers(); };
            _mainSurface.OnTextInput += ImguiRenderer.OnTextInput;
            _mainSurface.OnKeyDown += _layerManager.KeyDownLayers;
            _mainSurface.OnKeyUp += _layerManager.KeyUpLayers;


#if DISPLAY_DEBUG_INFO
            Console.WriteLine("████  █████ █   █  ███  ███ ████ \t");
            Console.WriteLine($"█   █ █     █   █ █   █  █  █   █ \tDevoid Version: {Engine.Instance.EngineVersion}");
            Console.WriteLine($"█   █ ████  █   █ █   █  █  █   █ \tRenderer: {Engine.Renderer.ActiveTechnique}");
            Console.WriteLine($"█   █ █      █ █  █   █  █  █   █ \tRendering Backend: {specification.Api}");
            Console.WriteLine("████  █████   █    ███  ███ ████ \t");
#endif

            Engine.Profiler.Initialize();

        }

        public void Run()
        {
            if (_surfaces.Count == 0)
                return;

            _layerManager.AttachLayers();
            while (_isRunning)
            {
                Engine.Profiler.BeginFrame();

                Engine.Profiler.Cpu.BeginScope("APPLICATION_LOOP");

                float timescale = Engine.Instance.TimeScale;
                float simulationTargetDeltaTime = 1 / Engine.Instance.SimulationTargetFramerate;
                float deltaTime = (float)_frameTimer.GetElapsedSeconds();
                _systemInfoTimer += deltaTime;
                Engine.Instance.FrameCount = _numFrames;

                _mainSurface.Window.PumpEvents();
                if (!_mainSurface.SkipRefresh)
                {
                    Engine.InputSystem.Update();
                }

                _deltaTimeAccumulator += Math.Min(deltaTime, 0.25f);
                while (_deltaTimeAccumulator >= simulationTargetDeltaTime)
                {
                    FixedUpdate(simulationTargetDeltaTime * timescale);
                    _deltaTimeAccumulator -= simulationTargetDeltaTime;
                }

                float alpha = _deltaTimeAccumulator / simulationTargetDeltaTime;
                alpha = Math.Clamp(alpha, 0f, 1f);
                Engine.Instance.InterpolationAlpha = alpha;

                Update(deltaTime * timescale);

                if (!_mainSurface.SkipRefresh)
                {
                    ICommandList cmd = Engine.GraphicsDevice.GetCommandList();
                    cmd.Begin();

                    cmd.SetFramebuffer(_mainSurface.Framebuffer);
                    cmd.ClearColor(0, Colors.Black);

                    ImguiRenderer.BeginFrame(_mainSurface, deltaTime);
                    UpdateCursor();
                    Render(cmd, _mainSurface);
                    Engine.InputSystem.EndFrame();

                    cmd.End();
                    Engine.GraphicsDevice.Submit(cmd);

                    _mainSurface.Present();
                }

                if (_mainSurface.Window.IsExiting)
                {
                    _mainSurface.Window.Close();
                    _mainSurface.Dispose();
                    _isRunning = false;
                }
                else if (!_mainSurface.Window.IsVisible)
                    _mainSurface.Window.IsVisible = true;

                _numFrames++;

                if (_systemInfoTimer >= 2.0f)
                {
                    _systemInfoTimer = 0.0f;

                    Engine.GraphicsDevice.UpdateMemoryInfo();
                }

                Engine.Profiler.Cpu.EndScope();

                if (_mainSurface.SkipRefresh)
                    Thread.Sleep(16);
            }

            // Application loop terminated.
            _layerManager.DetachLayers();
            Engine.Instance.ProjectSystem.Unload();
            Engine.Renderer.Dispose();
            Engine.GraphicsDevice.Dispose();
        }

        void FixedUpdate(float deltaTime)
        {
            _layerManager.FixedUpdateLayers(deltaTime);
        }

        void Update(float deltaTime)
        {
            _layerManager.UpdateLayers(deltaTime);
        }

        void Render(ICommandList cmd, WindowSurface surface)
        {
            _layerManager.RenderLayers(cmd);

            Engine.Renderer.PrepareGlobalFrame(cmd);
            Engine.Instance.ViewportManager.RenderAll(cmd);

            _layerManager.PostRenderLayers(cmd);
            ImguiRenderer.EndFrame(cmd, surface);
        }

        public void AddLayer(Layer layer)
        {
            layer.Application = this;
            _layerManager.AddLayer(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            _layerManager.RemoveLayer(layer);
        }

        void UpdateCursor()
        {
            if (Cursor.StateDirty)
            {
                MainWindow.Window!.CursorState = (OpenTK.Windowing.Common.CursorState)Cursor.CursorState;

                Cursor.StateDirty = false;
            }

            if (Cursor.ShapeDirty)
            {
                MainWindow!.Window!.Cursor = WindowUtil.ConvertCursorShape(Cursor.CursorShape);

                Cursor.ShapeDirty = false;
            }

            if (Cursor.PosDirty)
            {
                MainWindow!.Window.MousePosition = new OpenTK.Mathematics.Vector2(Cursor.MousePosition.X, Cursor.MousePosition.Y);
                Cursor.PosDirty = false;
            }
        }
    }
}
