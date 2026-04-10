using DevoidEngine.Engine.Content.Scenes;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidStandaloneLauncher.Prototypes;

namespace DevoidStandaloneLauncher
{
    public class PrototypeLoader : Layer
    {
        private float splashTimer = 0;
        private float splashDuration = 5f;
        private bool prototypeLoaded = false;

        internal Scene CurrentScene = SplashScene.CreateSplashScene(nameof(PlatformerTestExample));
        internal Prototype GamePrototype = new PlatformerTestExample();

        public override void OnGUIRender()
        {

        }

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

        public void SwitchPrototype(Prototype newPrototype)
        {
            // stop old scene
            CurrentScene?.Play(false);

            // create fresh scene
            CurrentScene = new Scene();

            // reset splash timer if needed
            splashTimer = 0f;

            // replace prototype
            GamePrototype = newPrototype;
            GamePrototype.loader = this;

            prototypeLoaded = false;

            GamePrototype.OnInit();
            prototypeLoaded = true;
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
            if (prototypeLoaded)
                GamePrototype.OnFixedUpdate(deltaTime);
        }

        public override void OnRender()
        {
            CurrentScene.Render();
            if (prototypeLoaded)
            {
                GamePrototype.OnRender();
            }
            Application.RenderScene();
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

        //public override void OnResize(int width, int height)
        //{
        //    CurrentScene?.ResizeCameras(width, height);
        //    if (prototypeLoaded)
        //    {
        //        GamePrototype.Resize(width, height);
        //    }
        //}
    }
}