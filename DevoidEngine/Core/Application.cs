

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


#if DISPLAY_DEBUG_INFO
            Console.WriteLine("==============================================");
            Console.WriteLine($"Devoid Version: {Engine.Instance.EngineVersion}");
            Console.WriteLine($"Renderer: {Engine.Renderer.ActiveTechnique}");
            Console.WriteLine($"Rendering Backend: {specification.API}");
            Console.WriteLine("==============================================");
#endif


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
                float targetDeltaTime = 1 / Engine.Instance.TargetFramerate;
                float deltaTime = (float)frameTimer.GetElapsedSeconds();
                systemInfoTimer += deltaTime;
                Engine.Instance.FrameCount = numFrames;

                foreach (var surface in surfaces)
                {
                    surface.Window.PumpEvents();
                    if (surface == mainSurface)
                        Engine.InputSystem.Update(); // Only update main window, change for multi window support
                }

                Engine.AudioSystem.Update();

                deltaTimeAccumulator += deltaTime;
                while (deltaTimeAccumulator >= targetDeltaTime)
                {
                    FixedUpdate(targetDeltaTime * timescale);
                    deltaTimeAccumulator -= targetDeltaTime;
                }

                float alpha = deltaTimeAccumulator / targetDeltaTime;
                alpha = Math.Clamp(alpha, 0f, 1f);
                Engine.Instance.InterpolationAlpha = alpha;

                Update(deltaTime * timescale);
                

                ICommandList cmd = Engine.GraphicsDevice.GetCommandList();
                cmd.Begin();

                foreach (var surface in surfaces)
                {
                    if (surface.SkipRefresh)
                        continue;
                    surface.UpdateSurface(deltaTime);
                    cmd.SetFramebuffer(surface.Framebuffer);
                    cmd.ClearColor(0, Colors.Black);
                    if (surface == mainSurface)
                    {
                        ImguiRenderer.BeginFrame(surface, deltaTime);
                        UpdateCursor();
                        Render(cmd, surface);
                        Engine.InputSystem.EndFrame();
                    }

                    surface.RenderSurface(cmd);

                }
                cmd.SetScissor(0, 0, 1280, 720);
                cmd.End();
                Engine.GraphicsDevice.Submit(cmd);

                foreach (var surface in surfaces)
                {
                    surface.Present();
                }

                Engine.GraphicsDevice.Submit(cmd);

                for (int i = surfaces.Count - 1; i >= 0; i--)
                {
                    var surface = surfaces[i];

                    if (surface.Window.IsExiting)
                    {
                        surface.Window.Close();
                        surface.Dispose();
                        surfaces.RemoveAt(i);
                    }
                    else if (!surface.Window.IsVisible)
                        surface.Window.IsVisible = true;
                }

                if (surfaces.Count == 0)
                {
                    isRunning = false;
                }

                numFrames++;

                if (systemInfoTimer >= 2.0f)
                {
                    systemInfoTimer = 0.0f;

                    Engine.GraphicsDevice.UpdateMemoryInfo();
                }

                Engine.Profiler.CPU.EndScope();
            }

            // Application loop terminated.
            layerManager.DetachLayers();
            Engine.Instance.SceneTree.Dispose();
            Engine.Instance.ProjectSystem.Unload();
            Engine.AudioSystem.Dispose();
            Engine.Renderer.Dispose();
            Engine.GraphicsDevice.Dispose();
        }

        void FixedUpdate(float deltaTime)
        {
            layerManager.FixedUpdateLayers(deltaTime);
            Engine.Instance.SceneTree.FixedUpdateScenes(deltaTime);
        }

        void Update(float deltaTime)
        {
            layerManager.UpdateLayers(deltaTime);

            List<Viewport> viewports = Engine.Instance.ViewportManager.GetViewports();
            Engine.UISystem.Update(deltaTime, viewports);
            Engine.GizmoSystem.Update(deltaTime, viewports);

            Engine.Instance.SceneTree.UpdateScenes(deltaTime);
        }

        void Render(ICommandList cmd, WindowSurface surface)
        {
            layerManager.RenderLayers(cmd);
            Engine.Instance.SceneTree.RenderScenes();

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
                MainWindow.Window!.CursorState = Cursor.cursorState;

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
