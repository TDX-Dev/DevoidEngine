using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.ProjectSystem;
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
                GC.Collect();
            }

            scene.Audio = EngineSingleton.Instance.AudioSystem;
            scene.Physics = EngineSingleton.Instance.PhysicsSystem;
            scene.ParticleSystem = EngineSingleton.Instance.ParticleSystem;
            // 2. Set new scene
            CurrentScene = scene;
        }

        public static void LoadStartupScene()
        {
            var project = ProjectManager.Current;

            if (project == null)
                throw new Exception("Project not loaded");

            string scenePath = project.Settings.StartupScene;

            if (string.IsNullOrEmpty(scenePath))
                throw new Exception("Startup scene not defined in project settings");

            var scene = Asset.Load<Scene>(scenePath);

            if (scene == null)
                throw new Exception($"Failed to load startup scene: {scenePath}");

            LoadScene(scene);
        }

    }
}
