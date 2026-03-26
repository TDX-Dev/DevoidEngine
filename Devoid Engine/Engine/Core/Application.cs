using DevoidEngine.Engine.Rendering;
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


        private FrameTimer frameTimer;
        private float targetFramerate = 60f;
        private float targetDeltaTime = 1 / 60f;
        private float deltaTimeAccumulator = 0f;
        private float timeScale = 1.0f;

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
            Renderer.GraphicsDevice = applicationSpecification.graphicsDevice;


            frameTimer = new FrameTimer();

            targetWindow = new Window(windowSpecification);

            targetWindow.Load();
            isRunning = true;
        }

        public void Run()
        {
            while (isRunning)
            {
                float deltaTime = (float)frameTimer.GetElapsedSeconds();
                targetWindow.ProcessEvents();


                deltaTimeAccumulator += deltaTime;
                while (deltaTimeAccumulator >= targetDeltaTime)
                {
                    FixedUpdate(targetDeltaTime * timeScale);
                    deltaTimeAccumulator -= targetDeltaTime;
                }

                Update(deltaTime * timeScale);
                Render();


                if (targetWindow.IsExiting)
                {
                    targetWindow.Close();
                    isRunning = false;
                }
            }
        }

        void FixedUpdate(float deltaTime)
        {

        }

        void Update(float deltaTime)
        {

        }

        void Render()
        {

        }


    }
}
