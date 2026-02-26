using DevoidEngine.Engine.Content.Scenes;
using DevoidEngine.Engine.Imgui;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using OpenTK.Windowing.Common;

namespace DevoidEngine.Engine.Core
{
    enum SupportedGraphicsBackends
    {
        DX11,
        OpenGL,
        OpenGLES
    }

    public struct ApplicationSpecification
    {
        public string Name;
        public int Width, Height;
        public bool forceVsync;
        public bool useImGui;
        public bool useImGuiDock;
        public bool useFullscreen;

        public bool useCustomTitlebar;
        public string customTitlebarLogo;
        public bool darkTitlebar;

        public IGraphicsDevice graphicsDevice;
    }

    public class Application
    {
        public WindowManager windowManager;
        public Window MainWindow;

        public IGraphicsDevice graphicsDevice;

        public LayerHandler LayerHandler;
        public ApplicationSpecification AppSpec;

        public ImGuiRenderer ImGuiRenderer;

        public void Create(ApplicationSpecification appSpec)
        {
            this.AppSpec = appSpec;

            // Main Window
            WindowSpecification windowSpecification = new WindowSpecification()
            {
                StartVisible = false,
                StartFocused = true,
                WindowSize = new System.Numerics.Vector2(appSpec.Width, appSpec.Height),
                WindowTitle = appSpec.Name
            };

            PresentationParameters presentationParameters = new PresentationParameters()
            {
                BackBufferWidth = appSpec.Width,
                BackBufferHeight = appSpec.Height,
                RefreshRate = 144,
                VSync = appSpec.forceVsync,
                BufferCount = 2,
                ColorFormat = TextureFormat.RGBA8_UNorm,
                Windowed = true
            };

            LayerHandler = new LayerHandler();


            windowManager = new WindowManager();
            windowManager.useVsyncLimiter = AppSpec.forceVsync;

            RenderThread.mainThreadID = windowManager.MainThreadID;

            MainWindow = new Window(windowSpecification);

            Screen.Size = new System.Numerics.Vector2(appSpec.Width, appSpec.Height);

            graphicsDevice = appSpec.graphicsDevice;
            graphicsDevice.Initialize(MainWindow.GetWindowPtr(), presentationParameters);
            Renderer.Initialize(graphicsDevice, appSpec.Width, appSpec.Height);
            UISystem.Initialize();

            MainWindow.OnLoad += OnLoad;
            MainWindow.OnUpdateFrame += OnUpdateFrame;
            MainWindow.OnRenderFrame += OnRenderFrame;

            MainWindow.KeyDown += OnKeyDown;
            MainWindow.KeyUp += OnKeyUp;

            MainWindow.Resize += OnResize;
            MainWindow.TextInput += OnTextInput;

            MainWindow.MouseMove += OnMouseMove;
            MainWindow.MouseDown += OnMouseInput;
            MainWindow.MouseEnter += OnMouseEnter;
            MainWindow.MouseLeave += OnMouseLeave;
            MainWindow.MouseUp += OnMouseInput;
            MainWindow.MouseWheel += OnMouseWheel;

            MainWindow.FocusedChanged += OnWindowFocusChange;

            windowManager.RegisterWindow(MainWindow);

            ImGuiRenderer = new ImGuiRenderer(graphicsDevice);
            ImGuiRenderer.Initialize();
            ImGuiRenderer.OnGUI += () => { LayerHandler.OnGUILayers(); };


        }

        private void OnMouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs obj)
        {
            LayerHandler.OnMouseMoveEvent(obj);
        }

        private void OnMouseWheel(OpenTK.Windowing.Common.MouseWheelEventArgs obj)
        {
            LayerHandler.OnMouseWheelEvent(obj);
        }

        private void OnMouseLeave()
        {

        }

        private void OnMouseEnter()
        {

        }

        private void OnMouseInput(OpenTK.Windowing.Common.MouseButtonEventArgs obj)
        {
            LayerHandler.OnMouseButtonEvent(obj);
        }

        private void OnTextInput(OpenTK.Windowing.Common.TextInputEventArgs obj)
        {
            LayerHandler.TextInput(obj.Unicode);

        }

        private void OnResize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            graphicsDevice.MainSurface.Resize(obj.Width, obj.Height);
            LayerHandler.ResizeLayers(obj.Width, obj.Height);
        }

        private void OnLoad()
        {
            LayerHandler.AttachLayers();
        }

        private void OnKeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            LayerHandler.KeyDownInput((Keys)obj.Key, obj.ScanCode, obj.Modifiers.ToString(), false);
        }

        private void OnKeyUp(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            LayerHandler.KeyUpInput((Keys)obj.Key, obj.ScanCode, obj.Modifiers.ToString(), false);
        }

        public void Run()
        {
            windowManager.RunTicked();
        }

        public void AddLayer(Layer layer)
        {
            layer.application = this;
            LayerHandler.AddLayer(layer);
        }
        private void OnRenderFrame(double deltaTime)
        {
            UpdateMainWindowState();

            ImGuiRenderer.PerFrame((float)deltaTime);

            LayerHandler.RenderLayers((float)deltaTime);

            FramePipeline.ExecuteRenderThread((float)deltaTime);

            LayerHandler.LateRenderLayers();

            graphicsDevice.MainSurface.Bind();
            graphicsDevice.MainSurface.Present();

            RenderThreadDispatcher.ExecutePending();
            Input.Publish();
        }

        private void OnUpdateFrame(double deltaTime)
        {

            Input.Update();

            ImGuiRenderer.UpdateInput();

            LayerHandler.UpdateLayers((float)deltaTime);

            UISystem.Update();

            FramePipeline.ExecuteUpdateThread((float)deltaTime);

            UpdateThreadDispatcher.ExecutePending();
        }

        private void OnWindowFocusChange(FocusedChangedEventArgs obj)
        {
            if (obj.IsFocused)
            {
                MainWindow.CursorState =
                    (OpenTK.Windowing.Common.CursorState)Cursor.cursorState;
            }
        }

        private void UpdateMainWindowState()
        {
            if (Cursor.isDirty && MainWindow.IsFocused)
            {
                MainWindow.CursorState =
                    (OpenTK.Windowing.Common.CursorState)Cursor.cursorState;

                Cursor.isDirty = false;
            }

        }
    }
}

