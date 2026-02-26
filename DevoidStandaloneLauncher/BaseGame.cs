//using DevoidEngine.Engine.Content.Scenes;
//using DevoidEngine.Engine.Core;
//using DevoidEngine.Engine.Rendering;
//using DevoidStandaloneLauncher.Prototypes;

//namespace DevoidStandaloneLauncher
//{
//    internal class BaseGame : Layer
//    {
//        private readonly Scene SplashScreen = SplashScene.CreateSplashScene();
//        public Scene MainScene = new Scene();
//        private readonly Prototype gamePrototype = new CubeSpinForwardRenderer();

//        private float _timer = 0;
//        private bool _isInitialized = false;

//        public override void OnAttach()
//        {
//            gamePrototype.baseLayer = this;
//            SceneManager.LoadScene(SplashScreen);
//            SplashScreen.Play();
//        }

//        void LoadPrototype()
//        {
//            SceneManager.LoadScene(MainScene);
//            gamePrototype.OnInit(MainScene);

//            MainScene.Play();
//        }

//        public override void OnUpdate(float deltaTime)
//        {
//            if (_isInitialized)
//            {
//                gamePrototype.OnUpdate(deltaTime);
//                MainScene.OnUpdate(deltaTime);
//                return;
//            }

//            _timer += deltaTime;
//            SplashScreen.OnUpdate(deltaTime);

//            if (_timer >= 0.5f)
//            {
//                _isInitialized = true;
//                LoadPrototype();
//            }

//        }

//        public override void OnRender(float deltaTime)
//        {
//            if (_isInitialized)
//            {
//                gamePrototype.OnRender(deltaTime);
//                MainScene.OnRender(deltaTime);
//                return;
//            }
//            SplashScreen.OnRender(deltaTime);

//        }

//        public override void OnLateRender()
//        {
//            Texture2D renderOutput = RenderBase.Output;
//            RenderAPI.RenderToScreen(renderOutput);

//        }

//        public override void OnResize(int width, int height)
//        {
//            Renderer.Resize(width, height);

//            Screen.Size = new System.Numerics.Vector2(width, height);
//            Renderer.graphicsDevice.SetViewport(0, 0, width, height);

//            MainScene.OnResize(width, height);
//        }

//        // ===============================
//        // RENDER THREAD INPUT EVENTS
//        // ===============================

//        public override void OnMouseMove(MouseMoveEvent e)
//        {
//            Input.OnMouseMove(e);
//        }

//        public override void OnMouseButton(MouseButtonEvent e)
//        {
//            Input.OnMouseButton(e);
//        }

//        public override void OnMouseWheel(MouseWheelEvent e)
//        {
//            Input.OnMouseWheel(e);
//        }

//        public override void OnKeyDown(KeyboardEvent e)
//        {
//            Input.OnKeyDown(e.Key);
//        }

//        public override void OnKeyUp(KeyboardEvent e)
//        {
//            Input.OnKeyUp(e.Key);
//        }
//    }
//}