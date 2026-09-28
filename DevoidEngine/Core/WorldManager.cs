namespace DevoidEngine.Core
{
    public class WorldManager
    {
        public List<World> Worlds = new(1);
        // It is entirely possible that there are no worlds active. during engine bootsplash. BUT that could be a bootsplash world in and of itself, and to be loaded by the runtime + editor.
        public World? ActiveWorld = null;

        public void Tick(float delta)
        {
            for (int i = 0; i < Worlds.Count; i++)
            {
                Worlds[i].TickWorld(delta);
            }
        }
    }
}
