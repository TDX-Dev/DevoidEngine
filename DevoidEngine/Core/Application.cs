

#define PROFILING

using DevoidEngine.Util;
using DevoidGPU;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
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
        private readonly List<WindowSurface> surfaces;
        private readonly FrameTimer frameTimer;

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

            var window = new Window(new WindowSpecification
            {
                Title = specification.Name,
                Width = specification.Width,
                Height = specification.Height,
                Resizable = specification.Resizable,
                StartVisible = false,
                StartFocused = true
            });

            var surface = new WindowSurface(
                window,
                Engine.GraphicsDevice,
                new SwapchainDescription()
                {
                    BufferCount = 2,
                    Format = DevoidGPU.TextureFormat.RGBA8_UNorm,
                    Height = 480,
                    Width = 640,
                    RefreshRate = new System.Numerics.Vector2(165, 0),
                    Samples = new DevoidGPU.TextureSampleDescription(1, 0),
                    VSync = false,
                    Windowed = true,
                }
            );

            surfaces.Add(surface);
        }

        public void Run()
        {
            if (surfaces.Count == 0)
                return;

            while (isRunning)
            {
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
                    //FixedUpdate(targetDeltaTime * timescale);
                    deltaTimeAccumulator -= targetDeltaTime;
                }

                float alpha = deltaTimeAccumulator / targetDeltaTime;
                alpha = Math.Clamp(alpha, 0f, 1f);
                Engine.Instance.InterpolationAlpha = alpha;

                foreach (var surface in surfaces)
                {
                    surface.Present();
                }


                foreach (var surface in surfaces)
                {
                    if (surface.Window.IsExiting)
                    {
                        surface.Window.Close();
                        isRunning = false;
                    }

                    if (!surface.Window.IsVisible)
                        surface.Window.IsVisible = true;
                }

                numFrames++;
            }
        }

    }
}
