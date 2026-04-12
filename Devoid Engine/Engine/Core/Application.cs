using Assimp;
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

        public bool allowResize;

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
        private Pool<CameraRenderContext> cameraRenderContextPool = new Pool<CameraRenderContext>();

        public ImGuiRenderer ImGuiBackend = null!;

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
                WindowState = applicationSpecification.useFullscreen ? WindowState.Fullscreen : WindowState.Normal,
                Resizeable = applicationSpecification.allowResize
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

            //Screen.Size = new Vector2(applicationSpecification.Width, applicationSpecification.Height);

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
                ImGuiBackend.OnTextInput((char)c.Unicode);
            };

            ImGuiBackend = new ImGuiRenderer(Renderer.GraphicsDevice, targetWindow);
            ImGuiBackend.Initialize();
            ImGuiBackend.OnGUI += () => { layerHandler.OnGUILayers(); };

            AddLayer(new DebugConsole());

            isRunning = true;
        }

        public void ApplyProjectSettings()
        {
            var project = ProjectManager.Current;
            if (project == null)
                throw new Exception("You need to initialize a project before trying to apply project settings");

            Input.LoadInputActions(project.Settings.InputActions);

            EngineSingleton.Instance.UseInterpolation = project.Settings.UsePhysicsInterpolation;
            TargetFrameRate = project.Settings.PhysicsUpdateFrequency;

            Screen.Size = new Vector2(project.Settings.RenderWidth, project.Settings.RenderHeight);
            Renderer.Resize(project.Settings.RenderWidth, project.Settings.RenderHeight);
            SceneManager.CurrentScene?.ResizeCameras(
                project.Settings.RenderWidth,
                project.Settings.RenderHeight
            );
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

            Renderer.GraphicsDevice.MainSurface.Resize(width, height);
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
                ImGuiBackend.BeginFrame(deltaTime);
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
                Resize();

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
            UpdateCursor();

            layerHandler.UpdateLayers(deltaTime);

            UISystem.Update(deltaTime);
        }

        void Render()
        {
            Renderer.GraphicsDevice.MainSurface.ClearColor(new Vector4(0,0,0,1));

            layerHandler.RenderLayers();
            // RenderScene must be called somewhere (or not) in RenderLayers();
            layerHandler.PostRenderLayers();
            ImGuiBackend.EndFrame();

            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.MainSurface.Present();
        }

        public void RenderScene()
        {
            if (SceneManager.CurrentScene != null)
            {
                List<IRenderComponent> renderables = SceneManager.CurrentScene.GetRenderables();
                List<CameraComponent3D> cameraComponents = SceneManager.CurrentScene.GetCameras3D();

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
                        ctx = cameraRenderContextPool.Get();
                        renderContexts.Add(ctx);
                    }

                    var cameraComponent = cameraComponents[i];

                    if (cameraComponent.Camera.RenderTarget == null)
                        continue;

                    ctx.camera = cameraComponent.Camera;
                    ctx.cameraData = cameraComponent.Camera.GetCameraData();
                    ctx.cameraTargetSurface = cameraComponent.Camera.RenderTarget;

                    foreach (var renderable in renderables)
                    {
                        renderable.Collect(cameraComponent.Camera, ctx);
                    }
                }
                for (int i = 0; i < cameraComponents.Count; i++)
                {
                    Renderer.Render(renderContexts[i]);
                }
            }
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

            if (Cursor.posDirty)
            {
                targetWindow!.MousePosition = new OpenTK.Mathematics.Vector2(Cursor.mousePosition.X, Cursor.mousePosition.Y);
                Console.WriteLine(Cursor.mousePosition);
                Cursor.posDirty = false;
            }
        }
    }
}
