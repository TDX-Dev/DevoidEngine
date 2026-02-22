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

        public event Action OnSplashEnd;

        public override void OnAttach()
        {
            splashScene = SplashScene.CreateSplashScene();
            SceneManager.LoadScene(splashScene);
        }

        public override void OnRender(float deltaTime)
        {
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

            OnSplashEnd?.Invoke();
        }
    }
}
