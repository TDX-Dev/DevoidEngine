using DevoidEngine.Rendering;

namespace DevoidEngine.Core
{
    public class SceneTree
    {
        public Viewport RootViewport { get; private set; } = null!;
        public Scene? CurrentScene { get; private set; }

        public ulong TickCount = 0;
        public ulong TickPhysicsCount = 0;

        public void Initialize()
        {
            RootViewport = new Viewport();
        }

        public Scene LoadScene(Scene scene)
        {
            if (CurrentScene != null)
            {
                CurrentScene.OnTreeExited();
                CurrentScene.Root.SetParent(null, true);
            }

            CurrentScene = scene;

            CurrentScene.Root.SetParent(RootViewport, true);
            RootViewport.TargetScene = CurrentScene;
            scene.OnTreeEntered();
            return scene;
        }

        public void Tick(float dt)
        {
            RootViewport.Tick(dt);
            TickCount++;
        }

        public void TickFixed(float dt)
        {
            RootViewport.TickFixed(dt);
            TickPhysicsCount++;
        }
    }
}
