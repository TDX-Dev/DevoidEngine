using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public static class SceneManager
    {
        public static Scene? CurrentScene;

        public static void LoadScene(Scene scene)
        {
            // 1. Destroy old scene
            if (CurrentScene != null)
            {
                //CurrentScene.Destroy();
                CurrentScene.Dispose();
            }

            scene.Audio = EngineSingleton.Instance.AudioSystem;
            // 2. Set new scene
            CurrentScene = scene;
        }

    }
}
