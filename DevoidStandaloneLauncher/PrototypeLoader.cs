using DevoidEngine.Engine.Content.Scenes;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidStandaloneLauncher.Prototypes;

namespace DevoidStandaloneLauncher
{
    public class PrototypeLoader : Layer
    {
        private float splashTimer = 0;
        private float splashDuration = 1f;
        private bool prototypeLoaded = false;

        internal Scene CurrentScene = SplashScene.CreateSplashScene(nameof(UITester));
        internal Prototype GamePrototype = new UITester();

        public override void OnAttach()
        {
            SceneManager.LoadScene(CurrentScene);
            CurrentScene.Play(true);
        }

        public void LoadPrototype()
        {
            GamePrototype.loader = this;
            GamePrototype.OnInit();
        }


        public override void OnUpdate(float deltaTime)
        {
            CurrentScene.Update(deltaTime);
            if (prototypeLoaded)
                GamePrototype.OnUpdate(deltaTime);

            if (splashTimer > splashDuration && !prototypeLoaded)
            {
                LoadPrototype();
                prototypeLoaded = true;
                return;
            }
            splashTimer += deltaTime;
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            CurrentScene.FixedUpdate(deltaTime);
        }

        public override void OnRender()
        {
            CurrentScene.Render();
            if (prototypeLoaded)
            {
                GamePrototype.OnRender();
            }

        }

        public override void OnPostRender()
        {
            if (prototypeLoaded)
            {
                GamePrototype.OnPostRender();
            }
            Texture2D renderOutput = (Texture2D)CurrentScene?.GetDefaultCamera3D()?.Camera?.RenderTarget?.GetRenderTexture(0);
            RenderAPI.RenderToScreen(renderOutput);
        }

        public override void OnResize(int width, int height)
        {
            CurrentScene?.ResizeCameras(width, height);
            if (prototypeLoaded)
            {
                GamePrototype.Resize(width, height);
            }
        }
    }
}