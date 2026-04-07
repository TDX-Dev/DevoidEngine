using System.Collections.Concurrent;

namespace DevoidEngine.Engine.Utilities
{
    public class Pool<T> where T : class, new()
    {
        public readonly ConcurrentQueue<T> objects;

        public Pool()
        {
            objects = new ConcurrentQueue<T>();
        }


        public T Get()
        {
            if (objects.TryDequeue(out var result))
                return result;

            return new T();
        }

        public void Return(T obj)
        {
            objects.Enqueue(obj);
        }

    }
}
