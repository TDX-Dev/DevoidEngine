namespace DevoidEngine.Core
{
    public class SceneTree
    {
        private readonly List<Scene> scenes;

        public SceneTree()
        {
            scenes = [];
        }


        public ulong TickCount = 0;
        public ulong TichPhysicsCount = 0;

        public void LoadSceneFromFile()
        {

        }

        public void LoadScene(Scene scene)
        {

        }

        public Scene NewScene(bool addToTree = false)
        {
            Scene scene = new(this);

            if (addToTree)
            {
                scenes.Add(scene);
                scene.OnTreeEntered();
            }

            return scene;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < scenes.Count; i++)
                scenes[i].Tick(dt);

            for (int i = 0; i < scenes.Count; i++)
                scenes[i].TickLate();


            TickCount++;
        }

        public void TickFixed(float dt)
        {
            for (int i = 0; i < scenes.Count; i++)
                scenes[i].TickFixed(dt);
        }

    }
}
