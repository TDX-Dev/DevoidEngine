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
            }

            // 2. Set new scene
            CurrentScene = newScene;

            // 3. Initialize (this may enqueue GPU commands)
            CurrentScene.Initialize();

            // 4. Insert GPU fence AFTER initialization
            var fence = Graphics.CreateFence();

            // 5. Wait until render thread has executed all prior GPU commands
            fence.Wait();
        }

        public static bool IsSceneLoaded()
        {
            return CurrentScene != null;
        }
    }
}
