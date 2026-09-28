using DevoidEngine.Entities;

namespace DevoidEngine.Core
{
    public sealed class World
    {
        public ulong CreationTick = 0;
        public List<BaseEntity> Entities = new(10000);

        public World()
        {

        }

        public void TickWorld(float delta)
        {
            TickEntities(delta);
        }

        void TickEntities(float delta)
        {
            for (int i = 0; i < Entities.Count; i++)
                Entities[i].OnTick(delta);
        }

        public T CreateEntity<T>() where T : BaseEntity, new()
        {
            T entity = new();

            Entities.Add(entity);

            entity.World = this;
            entity.CreationTick = Engine.Instance.FrameCount;

            entity.OnSpawn();

            return entity;
        }

        public void DestroyEntity(BaseEntity entity)
        {
            entity.OnDestroy();
            Entities.Remove(entity);
        }
    }
}
