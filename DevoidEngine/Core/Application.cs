

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
                StartCentered = true,
                StartFocused = true
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
                    RefreshRate = new System.Numerics.Vector2(165, 0),
                    Samples = new DevoidGPU.TextureSampleDescription(1, 0),
                    VSync = true,
                    Windowed = true
                }
            );

            surfaces.Add(mainSurface);

            for (int i = 0; i < 0; i++)
            {

                var window1 = new Window(new WindowSpecification
                {
                    Title = specification.Name,
                    Width = specification.Width,
                    Height = specification.Height,
                    Resizable = true,
                    StartVisible = false,
                    StartFocused = true,
                    StartCentered = true,
                    Transparency = true,
                });

                var surface1 = new WindowSurface(
                    window1,
                    Engine.GraphicsDevice,
                    new SwapchainDescription()
                    {
                        BufferCount = 2,
                        Format = TextureFormat.RGBA8_UNorm,
                        Height = 480,
                        Width = 640,
                        RefreshRate = new Vector2(165, 0),
                        Samples = new TextureSampleDescription(1, 0),
                        VSync = true,
                        Windowed = true,
                    }
                );

                surfaces.Add(surface1);
            }

            mesh = PrimitiveMeshes.GetCube();

            shader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/basic.dsd");

            ubo = new UniformBuffer(Engine.GraphicsDevice, ResourceUsage.Dynamic, 4);

            layout = Engine.GraphicsDevice.CreateDescriptorLayout(
            [
                new DescriptorBinding()
                {
                    Binding = 1,
                    Stages = DevoidGPU.ShaderStage.Fragment,
                    Type = DescriptorType.UniformBuffer
                }
            ]);

            set = Engine.GraphicsDevice.CreateDescriptorSet(layout);

            set.SetUniformBuffer(1, ubo.GPU);

            ubo.GPU.Update<uint>([64]);
        }

        readonly Mesh mesh;
        readonly Shader shader;
        readonly UniformBuffer ubo;
        readonly IDescriptorLayout layout;
        readonly IDescriptorSet set;

        public void Run()
        {
            if (surfaces.Count == 0)
                return;

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

                for (int i = 0; i < surfaces.Count; i++)
                {
                    var surface = surfaces[i];
                    if (i != 0) // Assume index 0 is always main window
                    {
                        surface.UpdateSurface(deltaTime * timescale);
                        surface.RenderSurface(cmd);
                    }
                    cmd.SetFramebuffer(surface.Framebuffer);
                    cmd.ClearColor(0, Colors.White);
                    Render(cmd);
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
        }

        void FixedUpdate(float deltaTime)
        {

        }

        void Update(float deltaTime)
        {

        }

        void Render(ICommandList cmd)
        {
            cmd.SetViewport(0, 0, 50, 100);
            cmd.SetPipeline(shader.GetPass("Forward").GetPipeline(Engine.GraphicsDevice, Vertex.VertexInfo));
            cmd.SetDescriptorSet(0, set);
            mesh.Draw(cmd);
        }
    }
}
