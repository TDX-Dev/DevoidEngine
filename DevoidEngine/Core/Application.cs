

#define PROFILING

using DevoidEngine.Util;
using DevoidGPU;
using SharpDX.DXGI;
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
        private readonly WindowSurface mainSurface;
        private readonly List<WindowSurface> surfaces;
        private readonly FrameTimer frameTimer;

        private readonly LayerManager layerManager;

        private float deltaTimeAccumulator = 0f;
        private uint numFrames = 0;
        private bool isRunning = true;

        public Application(ApplicationSpecification specification)
        {
            surfaces = [];
            frameTimer = new();

            EngineConfig configuration = new()
            {
                API = GraphicsAPI.DX11,
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
                
            });

            mainSurface = new WindowSurface(
                window,
                Engine.GraphicsDevice,
                new SwapchainDescription()
                {
                    BufferCount = 2,
                    Format = DevoidGPU.TextureFormat.RGBA8_UNorm,
                    Height = 480,
                    Width = 640,
                    RefreshRate = Vector2.Zero,
                    Samples = new DevoidGPU.TextureSampleDescription(1, 0),
                    VSync = specification.VSync,
                    Windowed = true
                }
            );

            surfaces.Add(mainSurface);

            //for (int i = 0; i < 10; i++)
            //{

            //    var window1 = new Window(new WindowSpecification
            //    {
            //        Title = specification.Name,
            //        Width = specification.Width,
            //        Height = specification.Height,
            //        Resizable = true,
            //        StartVisible = false,
            //        StartFocused = true,
            //        StartCentered = true,
            //        Transparency = true,
            //    });

            //    var surface1 = new WindowSurface(
            //        window1,
            //        Engine.GraphicsDevice,
            //        new SwapchainDescription()
            //        {
            //            BufferCount = 2,
            //            Format = TextureFormat.RGBA8_UNorm,
            //            Height = 480,
            //            Width = 640,
            //            RefreshRate = new Vector2(165, 0),
            //            Samples = new TextureSampleDescription(1, 0),
            //            VSync = true,
            //            Windowed = true,
            //        }
            //    );

            //    surfaces.Add(surface1);
            //}

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
                Engine.Instance.FrameCount = numFrames;

                foreach (var surface in surfaces)
                {
                    surface.Window.PumpEvents();
                }

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

                foreach (var surface in surfaces)
                {
                    surface.UpdateSurface(deltaTime);

                    cmd.SetFramebuffer(surface.Framebuffer);
                    cmd.ClearColor(0, Colors.White);

                    if (surface == mainSurface)
                    {
                        Render(cmd); // normal game rendering
                    }

                    surface.RenderSurface(cmd); // custom window rendering

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


                Engine.Profiler.CPU.EndScope();
            }
            layerManager.DetachLayers();
        }

        void FixedUpdate(float deltaTime)
        {
            layerManager.FixedUpdateLayers(deltaTime);
            Engine.Instance.SceneManager.FixedUpdateScenes(deltaTime);
        }

        void Update(float deltaTime)
        {
            layerManager.UpdateLayers(deltaTime);
            Engine.Instance.SceneManager.UpdateScenes(deltaTime);
        }

        void Render(ICommandList cmd)
        {
            layerManager.RenderLayers(cmd);
            Engine.Instance.SceneManager.RenderScenes();
        }

        public void AddLayer(Layer layer)
        {
            layerManager.AddLayer(layer);
        }

        public void RemoveLayer(Layer layer)
        {
            layerManager.RemoveLayer(layer);
        }
    }
}
