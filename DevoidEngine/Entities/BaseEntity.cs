using DevoidEngine.Core;

namespace DevoidEngine.Entities
{
    public class BaseEntity
    {
        public World World { get; internal set; } = null!;

        // Since im starting the system new with an entity system, here are the things i assume i'd need for entities.

        public ulong CreationTick;


        // I want this method to run, whenever the entity is added to the world.
        public virtual void OnSpawn() { }
        // On every update frame.
        public virtual void OnTick(float delta) { }
        // Called when the entity is removed.
        public virtual void OnDestroy() { }
    }
}
