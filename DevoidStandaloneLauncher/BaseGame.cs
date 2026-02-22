using DevoidEngine.Engine.Content.Splash;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidStandaloneLauncher.Prototypes;

namespace DevoidStandaloneLauncher
{
    internal class BaseGame : Layer
    {
        private readonly Scene MainScene = new Scene();
        private readonly Prototype gamePrototype = new CubeSpinForwardRenderer();
        private readonly SplashLayer splashLayer = new SplashLayer();
        private bool isInitialized = false;

        public override void OnAttach()
        {
            application.AddLayer(splashLayer);
            splashLayer.OnSplashEnd += () =>
            {
                SceneManager.LoadScene(MainScene);

                gamePrototype.OnInit(MainScene);
                isInitialized = true;
            };
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!isInitialized) return;
            // Game logic uses stable snapshot
            gamePrototype.OnUpdate(deltaTime);
            //MainScene.OnUpdate(deltaTime);

        }

        public override void OnRender(float deltaTime)
        {
            if (!isInitialized) return;
            gamePrototype.OnRender(deltaTime);
           // MainScene.OnRender(deltaTime);

        }

        public override void OnLateRender()
        {

            Texture2D renderOutput = RenderBase.Output;
            RenderAPI.RenderToScreen(renderOutput);

        }

        public override void OnResize(int width, int height)
        {
            Renderer.Resize(width, height);

            Screen.Size = new System.Numerics.Vector2(width, height);
            Renderer.graphicsDevice.SetViewport(0, 0, width, height);

            //MainScene.OnResize(width, height);
        }

        // ===============================
        // RENDER THREAD INPUT EVENTS
        // ===============================

        public override void OnMouseMove(MouseMoveEvent e)
        {
            Input.OnMouseMove(e);
        }

        public override void OnMouseButton(MouseButtonEvent e)
        {
            Input.OnMouseButton(e);
        }

        public override void OnMouseWheel(MouseWheelEvent e)
        {
            Input.OnMouseWheel(e);
        }

        public override void OnKeyDown(KeyboardEvent e)
        {
            Input.OnKeyDown(e.Key);
        }

        public override void OnKeyUp(KeyboardEvent e)
        {
            Input.OnKeyUp(e.Key);
        }
    }
}
