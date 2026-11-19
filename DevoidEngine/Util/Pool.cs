using System.Collections.Concurrent;

namespace DevoidEngine.Util
{
    public class Pool<T> where T : class, new()
    {
        public readonly ConcurrentQueue<T> Objects;

        public Pool()
        {
            Objects = new ConcurrentQueue<T>();
        }


        public T Get()
        {
            if (Objects.TryDequeue(out var result))
                return result;

            return new T();
        }

        public void Return(T obj)
        {
            Objects.Enqueue(obj);
        }

    }
}
