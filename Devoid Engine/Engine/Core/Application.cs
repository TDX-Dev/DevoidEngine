using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.AudioSystem;
using DevoidEngine.Engine.AudioSystem.SoLoud;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.DebugTools;
using DevoidEngine.Engine.Imgui;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Physics.Bepu;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
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

        //private ImGuiRenderer imGuiRenderer;

        private bool isRunning = false;
        private bool isResizePending = false;
        public int pendingWidth;
        public int pendingHeight;
        private Window? targetWindow;

        public Application()
        {
            EngineSingleton EngineSingleton = new EngineSingleton();
            layerHandler = new LayerHandler();
            frameTimer = new FrameTimer();
        }

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

            EngineSingleton.Instance.AudioSystem = new AudioManager(new SoLoudAudioBackend());
            EngineSingleton.Instance.PhysicsSystem = new PhysicsSystem(new BepuPhysicsBackend());
            EngineSingleton.Instance.TargetFrameRate = targetFramerate;

            Screen.Size = new Vector2(applicationSpecification.Width, applicationSpecification.Height);

            Renderer.GraphicsDevice = applicationSpecification.graphicsDevice;
            RenderThread.mainThreadID = Thread.CurrentThread.ManagedThreadId;

            targetWindow = new Window(windowSpecification);
            Renderer.GraphicsDevice.Initialize(targetWindow.GetWindowPtr(), presentParameters);
            Renderer.Initialize(applicationSpecification.Width, applicationSpecification.Height);

            Input.Initialize(targetWindow);
            UISystem.Initialize();


            targetWindow.OnWindowResize += HandleWindowResize;
            targetWindow.TextInput += c =>
            {
                UISystem.TextInput((char)c.Unicode);
            };

            //imGuiRenderer = new ImGuiRenderer(Renderer.GraphicsDevice);
            //imGuiRenderer.Initialize();
            //imGuiRenderer.OnGUI += () => { layerHandler.OnGUILayers(); };

            AddLayer(new DebugConsole());

            isRunning = true;
        }

        public void AddLayer(Layer layer)
        {
            layer.Application = this;
            layerHandler.AddLayer(layer);
        }
        public void RemoveLayer(Layer layer) => layerHandler.RemoveLayer(layer);

        private void HandleWindowResize(int width, int height)
        {
            if (width <= 0 || height <= 0) return;

            pendingWidth = width;
            pendingHeight = height;
            isResizePending = true;
        }

        private void Resize()
        {
            if (!isResizePending) return;
            int width = pendingWidth;
            int height = pendingHeight;
            Screen.Size = new Vector2(width, height);
            Renderer.Resize(width, height);
            layerHandler.ResizeLayers(width, height);
            isResizePending = false;
        }

        public void Quit()
        {
            targetWindow?.Close();
        }

        public void Run()
        {
            if (targetWindow == null) return;
            layerHandler.AttachLayers();
            while (isRunning)
            {

                float deltaTime = (float)frameTimer.GetElapsedSeconds();
                EngineSingleton.Instance.FrameCount = numFrames;

                RenderThread.Execute();

                targetWindow.ProcessEvents();
                Input.Update();
                EngineSingleton.Instance.AudioSystem.Update();



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

                //imGuiRenderer.PerFrame(deltaTime);
                Input.EndFrame();
                RenderThread.ExecuteFrameEnd();


                if (targetWindow.IsExiting)
                {
                    targetWindow.Close();
                    isRunning = false;
                }
                if (!targetWindow.IsVisible)
                    targetWindow.IsVisible = true;
                Resize();

                numFrames++;


            }
            layerHandler.DetachLayers();
        }

        void FixedUpdate(float deltaTime)
        {
            layerHandler.FixedUpdateLayers(deltaTime);
            EngineSingleton.Instance.PhysicsSystem.Step(deltaTime);
            EngineSingleton.Instance.PhysicsSystem.SyncTransforms(deltaTime);
            EngineSingleton.Instance.PhysicsSystem.ResolveFrameCollisions();
        }

        void Update(float deltaTime)
        {
            UpdateCursor();

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

                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < cameraComponents.Count; i++)
                {
                    CameraRenderContext ctx;

                    if (i < renderContexts.Count)
                    {
                        ctx = renderContexts[i];
                        ctx.Clear();
                    }
                    else
                    {
                        ctx = new CameraRenderContext();
                        renderContexts.Add(ctx);
                    }

                    var cameraComponent = cameraComponents[i];
                    ctx.cameraData = cameraComponent.Camera.GetCameraData();
                    if (cameraComponent.Camera.RenderTarget == null)
                        return;
                    ctx.cameraTargetSurface = cameraComponent.Camera.RenderTarget;


                    foreach (var renderable in renderables)
                    {
                        renderable.Collect(cameraComponent, ctx);
                    }

                    renderContexts.Add(ctx);

                }
                long after = GC.GetAllocatedBytesForCurrentThread();

                for (int i = 0; i < renderContexts.Count; i++)
                {
                    var ctx = renderContexts[i];
                    Renderer.Render(ctx);
                }
                Console.WriteLine(after - before);
            }

            layerHandler.PostRenderLayers();

            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.MainSurface.Present();
        }

        void UpdateCursor()
        {
            if (Cursor.stateDirty)
            {
                targetWindow!.CursorState =
                    (OpenTK.Windowing.Common.CursorState)Cursor.cursorState;

                Cursor.stateDirty = false;
            }

            if (Cursor.shapeDirty)
            {
                targetWindow!.Cursor = Window.ConvertCursorShape(Cursor.cursorShape);

                Cursor.shapeDirty = false;
            }
        }
    }
}
