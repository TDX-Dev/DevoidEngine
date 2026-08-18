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
                CurrentScene.Dispose();
                GC.Collect();
            }

            CurrentScene = scene;

            scene.Physics = new PhysicsSystem(new BepuPhysicsBackend());
            scene.Audio = Engine.AudioSystem;

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
