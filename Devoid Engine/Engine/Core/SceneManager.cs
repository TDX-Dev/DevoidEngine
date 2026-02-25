using DevoidEngine.Engine.Rendering;

namespace DevoidEngine.Engine.Core
{
    public static class SceneManager
    {
        public static bool Enabled = true;
        public static Scene CurrentScene;

        static SceneManager()
        {

        }

        public static void LoadScene(Scene newScene)
        {
            // 1. Destroy old scene
            if (CurrentScene != null)
            {
                CurrentScene.Destroy();
                CurrentScene.Dispose();
                FramePipeline.Reset();
            }

            // 2. Set new scene
            CurrentScene = newScene;

            // 3. Initialize (this may enqueue GPU commands)
            CurrentScene.Initialize();

            if (Graphics.MainThreadStarted)
            {
                var fence = Graphics.CreateFence();
                fence.Wait();
            }
        }

        public static bool IsSceneLoaded()
        {
            return CurrentScene != null;
        }
    }
}
