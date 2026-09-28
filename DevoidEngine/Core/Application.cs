

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

        public GraphicsAPI API;
    }

    public class Application
    {
        public const int ENGINE_MAJOR_VER = 0;
        public const int ENGINE_MINOR_VER = 1;

        public WindowSurface MainWindow => mainSurface;
        public readonly ImGuiRenderer ImguiRenderer;

        private readonly WindowSurface mainSurface;
        private readonly List<WindowSurface> surfaces;
        private readonly FrameTimer frameTimer;

        private readonly LayerManager layerManager;
        private float deltaTimeAccumulator = 0f;
        private uint numFrames = 0;
        private bool isRunning = true;
        private float systemInfoTimer;

        public Application(ApplicationSpecification specification)
        {
            surfaces = [];
            frameTimer = new();

            EngineConfig configuration = new()
            {
                API = GraphicsAPI.DX11,
                RendererConfig = new RendererConfig()
                {
                    Technique = RenderTechnique.Forward
                },
                EngineVersion = new Version(ENGINE_MAJOR_VER, ENGINE_MINOR_VER)
            };

            Engine.Initialize(configuration);

            layerManager = new LayerManager();

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

            mainSurface = new WindowSurface(
                window,
                Engine.GraphicsDevice,
                new SwapchainDescription()
                {
                    BufferCount = 2,
                    Format = DevoidGPU.TextureFormat.RGBA8_UNorm,
                    Height = specification.Height,
                    Width = specification.Width,
                    RefreshRate = Vector2.Zero,
                    Samples = new DevoidGPU.TextureSampleDescription(1, 0),
                    VSync = specification.VSync,
                    Windowed = true
                }
            );

            surfaces.Add(mainSurface);

            Engine.InputSystem.UpdateInputProviderWindow(window);

            ImguiRenderer = new ImGuiRenderer();
            ImguiRenderer.Initialize(mainSurface);
            ImguiRenderer.OnGUI += () => { layerManager.OnGUILayers(); };
            mainSurface.OnTextInput += ImguiRenderer.OnTextInput;
            mainSurface.OnKeyDown += layerManager.KeyDownLayers;
            mainSurface.OnKeyUp += layerManager.KeyUpLayers;


#if DISPLAY_DEBUG_INFO
            Console.WriteLine("████  █████ █   █  ███  ███ ████ \t");
            Console.WriteLine($"█   █ █     █   █ █   █  █  █   █ \tDevoid Version: {Engine.Instance.EngineVersion}");
            Console.WriteLine($"█   █ ████  █   █ █   █  █  █   █ \tRenderer: {Engine.Renderer.ActiveTechnique}");
            Console.WriteLine($"█   █ █      █ █  █   █  █  █   █ \tRendering Backend: {specification.API}");
            Console.WriteLine("████  █████   █    ███  ███ ████ \t");
#endif

            Engine.Profiler.Initialize();

        }

        public void Run()
        {
            if (surfaces.Count == 0)
                return;

            layerManager.AttachLayers();
            while (isRunning)
            {
                Engine.Profiler.BeginFrame();

                Engine.Profiler.CPU.BeginScope("APPLICATION_LOOP");

                float timescale = Engine.Instance.TimeScale;
                float simulationTargetDeltaTime = 1 / Engine.Instance.SimulationTargetFramerate;
                float deltaTime = (float)frameTimer.GetElapsedSeconds();
                systemInfoTimer += deltaTime;
                Engine.Instance.FrameCount = numFrames;

                mainSurface.Window.PumpEvents();
                if (!mainSurface.SkipRefresh)
                {
                    Engine.InputSystem.Update();
                }

                deltaTimeAccumulator += Math.Min(deltaTime, 0.25f);
                while (deltaTimeAccumulator >= simulationTargetDeltaTime)
                {
                    FixedUpdate(simulationTargetDeltaTime * timescale);
                    deltaTimeAccumulator -= simulationTargetDeltaTime;
                }

                float alpha = deltaTimeAccumulator / simulationTargetDeltaTime;
                alpha = Math.Clamp(alpha, 0f, 1f);
                Engine.Instance.InterpolationAlpha = alpha;

                Update(deltaTime * timescale);

                if (!mainSurface.SkipRefresh)
                {
                    ICommandList cmd = Engine.GraphicsDevice.GetCommandList();
                    cmd.Begin();

                    cmd.SetFramebuffer(mainSurface.Framebuffer);
                    cmd.ClearColor(0, Colors.Black);

                    ImguiRenderer.BeginFrame(mainSurface, deltaTime);
                    UpdateCursor();
                    Render(cmd, mainSurface);
                    Engine.InputSystem.EndFrame();

                    cmd.End();
                    Engine.GraphicsDevice.Submit(cmd);

                    mainSurface.Present();
                }

                if (mainSurface.Window.IsExiting)
                {
                    mainSurface.Window.Close();
                    mainSurface.Dispose();
                    isRunning = false;
                }
                else if (!mainSurface.Window.IsVisible)
                    mainSurface.Window.IsVisible = true;

                numFrames++;

                if (systemInfoTimer >= 2.0f)
                {
                    systemInfoTimer = 0.0f;

                    Engine.GraphicsDevice.UpdateMemoryInfo();
                }

                Engine.Profiler.CPU.EndScope();

                if (mainSurface.SkipRefresh)
                    Thread.Sleep(16);
            }

            // Application loop terminated.
            layerManager.DetachLayers();
            Engine.Instance.ProjectSystem.Unload();
            Engine.Renderer.Dispose();
            Engine.GraphicsDevice.Dispose();
        }

        void FixedUpdate(float deltaTime)
        {
            layerManager.FixedUpdateLayers(deltaTime);
        }

        void Update(float deltaTime)
        {
            layerManager.UpdateLayers(deltaTime);
        }

        void Render(ICommandList cmd, WindowSurface surface)
        {
            layerManager.RenderLayers(cmd);

            Engine.Renderer.PrepareGlobalFrame(cmd);
            Engine.Instance.ViewportManager.RenderAll(cmd);

            layerManager.PostRenderLayers(cmd);
            ImguiRenderer.EndFrame(cmd, surface);
        }

        public void AddLayer(Layer layer)
        {
            layer.Application = this;
            layerManager.AddLayer(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            layerManager.RemoveLayer(layer);
        }

        void UpdateCursor()
        {
            if (Cursor.stateDirty)
            {
                MainWindow.Window!.CursorState = (OpenTK.Windowing.Common.CursorState)Cursor.cursorState;

                Cursor.stateDirty = false;
            }

            if (Cursor.shapeDirty)
            {
                MainWindow!.Window!.Cursor = WindowUtil.ConvertCursorShape(Cursor.cursorShape);

                Cursor.shapeDirty = false;
            }

            if (Cursor.posDirty)
            {
                MainWindow!.Window.MousePosition = new OpenTK.Mathematics.Vector2(Cursor.mousePosition.X, Cursor.mousePosition.Y);
                Cursor.posDirty = false;
            }
        }
    }
}
