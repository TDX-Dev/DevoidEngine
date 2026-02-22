namespace DevoidEngine.Engine.Core
{
    public static class SceneManager
    {
        public static Scene MainScene;

        static SceneManager()
        {

        }

        public static void LoadScene(Scene newScene)
        {

            if (MainScene != null)
            {
                MainScene.Destroy();
                MainScene.Dispose();
            }

            MainScene = newScene;

            MainScene.Initialize();
            MainScene.Play();
        }

        public static void Update(float delta)
        {
            MainScene?.OnUpdate(delta);
        }

        public static void Render(float delta)
        {
            MainScene?.OnRender(delta);
        }

        public static bool IsSceneLoaded()
        {
            return MainScene != null;
        }
    }
}
