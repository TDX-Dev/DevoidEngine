using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class SceneManager
    {
        public Scene? CurrentScene { get; private set; }
        public void LoadScene(Scene scene)
        {
            if (CurrentScene != null)
            {
                //CurrentScene.Destroy();
                CurrentScene.Dispose();
                GC.Collect();
            }

            //scene.Audio = EngineSingleton.Instance.AudioSystem;
            //scene.Physics = EngineSingleton.Instance.PhysicsSystem;
            //scene.ParticleSystem = EngineSingleton.Instance.ParticleSystem;
            CurrentScene = scene;
            CurrentScene.Start();
        }

        public void UpdateScenes(float dt)
        {
            CurrentScene?.Update(dt);
        }

        public void FixedUpdateScenes(float dt)
        {
            CurrentScene?.FixedUpdate(dt);
        }

        public void RenderScenes()
        {
            CurrentScene?.Render();
        }
    }
}
