using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Diagnostics;
using System.Numerics;

namespace DevoidEngine.Engine.Core
{
    public struct ApplicationSpecification
    {
        public string Name;
        public int Width, Height;
        public bool forceVsync;
        public bool useFullscreen;

        public bool useCustomTitlebar;
        public string customTitlebarLogo;
        public bool darkTitlebar;
        public bool useImGui;
        public bool useImGuiDock;
        public bool useDebugConsole;


        public IGraphicsDevice graphicsDevice;
    }

    public class Application
    {
        public float TargetFrameRate
        {
            get => targetFramerate;
            set
            {
                targetFramerate = value;
                targetDeltaTime = 1 / targetFramerate;
            }
        }

        private LayerHandler layerHandler;
        private FrameTimer frameTimer;
        private float targetFramerate = 60f;
        private float targetDeltaTime = 1 / 60f;
        private float deltaTimeAccumulator = 0f;
        private float timeScale = 1.0f;
        private uint numFrames = 0;

        private List<CameraRenderContext> renderContexts = new List<CameraRenderContext>();

        private bool isRunning = false;
        private Window targetWindow;

        public void Initialize(ApplicationSpecification applicationSpecification)
        {
            WindowSpecification windowSpecification = new WindowSpecification()
            {
                WindowTitle = applicationSpecification.Name,
                StartFocused = true,
                StartVisible = false,
                VSync = applicationSpecification.forceVsync ? VSyncMode.Adaptive : VSyncMode.Off,
                WindowSize = new Vector2(applicationSpecification.Width, applicationSpecification.Height),
                WindowMinimumSize = new Vector2(100, 100),
                WindowState = applicationSpecification.useFullscreen ? WindowState.Fullscreen : WindowState.Normal
            };

            PresentationParameters presentParameters = new PresentationParameters()
            {
                VSync = applicationSpecification.forceVsync,
                ColorFormat = TextureFormat.RGBA8_UNorm,
                BufferCount = 2,
                BackBufferWidth = applicationSpecification.Width,
                BackBufferHeight = applicationSpecification.Height,
                Windowed = true,
            };

            EngineSingleton EngineSingleton = new EngineSingleton();
            layerHandler = new LayerHandler();
            Screen.Size = new Vector2(applicationSpecification.Width, applicationSpecification.Height);

            Renderer.GraphicsDevice = applicationSpecification.graphicsDevice;
            RenderThread.mainThreadID = Thread.CurrentThread.ManagedThreadId;

            frameTimer = new FrameTimer();

            targetWindow = new Window(windowSpecification);
            Renderer.GraphicsDevice.Initialize(targetWindow.GetWindowPtr(), presentParameters);
            Renderer.Initialize(applicationSpecification.Width, applicationSpecification.Height);

            Input.Initialize(targetWindow);
            UISystem.Initialize();


            targetWindow.OnResize += HandleWindowResize;

            isRunning = true;
        }

        public void AddLayer(Layer layer) => layerHandler.AddLayer(layer);
        public void RemoveLayer(Layer layer) => layerHandler.RemoveLayer(layer);

        private void HandleWindowResize(int width, int height)
        {
            if (width <= 0 || height <= 0) return;

            // 1. Update global screen size
            Screen.Size = new Vector2(width, height);

            // 2. Resize renderer / swapchain
            Renderer.Resize(width, height);

            //UISystem.Resize(width, height);

            // 4. Notify layers
            layerHandler.ResizeLayers(width, height);
        }

        public void Run()
        {
            layerHandler.AttachLayers();
            while (isRunning)
            {
                float deltaTime = (float)frameTimer.GetElapsedSeconds();
                EngineSingleton.Instance.FrameCount = numFrames;

                RenderThread.Execute();

                targetWindow.ProcessEvents();
                Input.Update();


                deltaTimeAccumulator += deltaTime;
                while (deltaTimeAccumulator >= targetDeltaTime)
                {
                    FixedUpdate(targetDeltaTime * timeScale);
                    deltaTimeAccumulator -= targetDeltaTime;
                }

                float alpha = deltaTimeAccumulator / targetDeltaTime;
                alpha = Math.Clamp(alpha, 0f, 1f);
                EngineSingleton.Instance.InterpolationAlpha = alpha;


                Update(deltaTime * timeScale);
                Render();


                Input.EndFrame();
                RenderThread.ExecuteFrameEnd();

                if (targetWindow.IsExiting)
                {
                    targetWindow.Close();
                    isRunning = false;
                }
                if (!targetWindow.IsVisible)
                    targetWindow.IsVisible = true;

                numFrames++;
            }
            layerHandler.DetachLayers();
        }

        void FixedUpdate(float deltaTime)
        {
            layerHandler.FixedUpdateLayers(deltaTime);
        }

        void Update(float deltaTime)
        {
            if (Cursor.isDirty)
            {
                targetWindow.CursorState = (OpenTK.Windowing.Common.CursorState)Cursor.cursorState;
                Cursor.isDirty = false;
            }

            layerHandler.UpdateLayers(deltaTime);
            UISystem.Update(deltaTime);
        }

        void Render()
        {
            layerHandler.RenderLayers();

            if (SceneManager.CurrentScene != null)
            {
                List<IRenderComponent> renderables = SceneManager.CurrentScene.GetRenderables();
                List<CameraComponent3D> cameraComponents = SceneManager.CurrentScene.GetCameras3D();
                renderContexts.Clear();

                for (int i = 0; i < cameraComponents.Count; i++)
                {
                    var cameraComponent = cameraComponents[i];

                    CameraRenderContext ctx = new CameraRenderContext();
                    ctx.cameraData = cameraComponent.Camera.GetCameraData();
                    ctx.cameraTargetSurface = cameraComponent.Camera.RenderTarget;

                    foreach (var renderable in renderables)
                    {
                        renderable.Collect(cameraComponent, ctx);
                    }

                    renderContexts.Add(ctx);

                }

                for (int i = 0; i < renderContexts.Count; i++)
                {
                    var ctx = renderContexts[i];
                    Renderer.Render(ctx);
                }
            }

            layerHandler.PostRenderLayers();

            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.MainSurface.Present();
        }


    }
}
