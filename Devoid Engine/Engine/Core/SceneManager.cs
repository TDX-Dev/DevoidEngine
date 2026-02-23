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

            if (CurrentScene != null)
            {
                CurrentScene.Destroy();
                CurrentScene.Dispose();
            }

            CurrentScene = newScene;

            CurrentScene.Initialize();
            //MainScene.Play();
        }

        public static bool IsSceneLoaded()
        {
            return CurrentScene != null;
        }
    }
}
