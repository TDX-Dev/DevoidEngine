using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
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
