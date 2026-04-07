using System.Collections.Concurrent;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    class RenderCommandPool<T> where T : RenderCommand, new()
    {
        private readonly ConcurrentBag<T> _pool = new();

        public T Get()
        {
            if (_pool.TryTake(out var cmd))
                return cmd;

            var newCmd = new T();
            newCmd.ReturnToPool = cmd => Return((T)cmd);
            return newCmd;
        }

        public void Return(T cmd)
        {
            _pool.Add(cmd);
        }
    }
}