using DevoidEngine.Engine.Content.Scenes;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Content.Splash
{
    public class SplashLayer : Layer
    {
        private float timer;
        private float duration = 3f;
        private Scene splashScene;

        public override void OnAttach()
        {
            splashScene = SplashScene.CreateSplashScene();
            SceneManager.Enabled = true;
            splashScene.Initialize();
            splashScene.Play();
        }

        public override void OnUpdate(float deltaTime)
        {
            splashScene.OnUpdate(deltaTime);
        }

        public override void OnRender(float deltaTime)
        {
            Console.WriteLine("Hey!");
            splashScene.OnRender(deltaTime);
            timer += deltaTime;

            if (timer >= duration)
            {
                TransitionToMainScene();
            }
        }

        private void TransitionToMainScene()
        {

            // Remove splash layer so it doesn't run again
            application.LayerHandler.RemoveLayer(this);
            SceneManager.Enabled = true;
        }
    }
}
