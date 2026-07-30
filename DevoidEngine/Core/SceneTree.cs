using DevoidEngine.Rendering;

namespace DevoidEngine.Core
{
    public class SceneTree
    {
        public Viewport RootViewport { get; set; }
        public Scene? CurrentScene { get; private set; }

        public event Action<Scene>? OnSceneChanged;

        private readonly List<Viewport> viewports = [];

        public SceneTree()
        {
            RootViewport = new Viewport();
            viewports.Add(RootViewport);
        }

        public List<Viewport> GetViewports()
        {
            return viewports;
        }

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
            scene.Physics = Engine.PhysicsSystem;
            scene.Audio = Engine.AudioSystem;
            CurrentScene.Start();

            OnSceneChanged?.Invoke(CurrentScene);
        }

        public void UpdateScenes(float dt)
        {
            CurrentScene?.Update(dt);
            CurrentScene?.LateUpdate(dt);
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
