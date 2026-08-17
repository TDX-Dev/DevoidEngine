using DevoidEngine.Audio;
using DevoidEngine.Audio.SoLoud;
using DevoidEngine.Physics;
using DevoidEngine.Physics.Bepu;
using DevoidEngine.Rendering;

namespace DevoidEngine.Core
{
    public sealed class SceneTree : IDisposable
    {
        public Viewport RootViewport { get; set; }
        public Scene? CurrentScene { get; private set; }

        public event Action<Scene>? OnSceneChanged;

        public SceneTree()
        {
            RootViewport = new Viewport();
            Engine.Instance.ViewportManager.RegisterViewport(RootViewport);
        }

        public void LoadScene(Scene scene, bool disposeOld = true)
        {
            if (CurrentScene != null && disposeOld)
            {
                //CurrentScene.Destroy();
                CurrentScene.Dispose();
                GC.Collect();
            }

            //scene.Audio = EngineSingleton.Instance.AudioSystem;
            //scene.Physics = EngineSingleton.Instance.PhysicsSystem;
            //scene.ParticleSystem = EngineSingleton.Instance.ParticleSystem;
            CurrentScene = scene;
            
            if (!scene.IsStarted)
            {
                scene.Physics = new PhysicsSystem(new BepuPhysicsBackend());
                scene.Audio = Engine.AudioSystem;
                scene.Start();
            }

            RootViewport.TargetScene = scene;

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

        public void QueueFree(Scene scene)
        {
            scene.Dispose();
            GC.Collect();
        }

        public void Dispose()
        {
            CurrentScene?.Dispose();
            RootViewport.Dispose();
        }
    }
}
