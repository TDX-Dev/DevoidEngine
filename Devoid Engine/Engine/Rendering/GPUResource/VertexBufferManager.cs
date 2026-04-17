using DevoidEngine.Engine.Core;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class VertexBufferManager
    {
        private uint _nextVertexBufferHandleID = 0;

        internal Dictionary<uint, IVertexBuffer> _vertexBuffers = new();

        private RenderCommandPool<CreateVertexBufferCommand> _createPool;
        private Dictionary<Type, object> _setDataPools = new();
        private RenderCommandPool<BindVertexBufferCommand> _bindPool;
        private RenderCommandPool<BindVertexBuffersCommand> _bindMultiplePool;
        private RenderCommandPool<DeleteVertexBufferCommand> _deletePool;

        public VertexBufferManager()
        {
            _createPool = new();
            _setDataPools = new();
            _bindPool = new();
            _bindMultiplePool = new();
            _deletePool = new();
        }

        private RenderCommandPool<SetVertexBufferDataCommand<T>> GetSetDataPool<T>() where T : struct
        {
            var type = typeof(T);

            if (!_setDataPools.TryGetValue(type, out var poolObj))
            {
                var newPool = new RenderCommandPool<SetVertexBufferDataCommand<T>>();
                _setDataPools[type] = newPool;
                return newPool;
            }

            return (RenderCommandPool<SetVertexBufferDataCommand<T>>)poolObj;
        }


        public VertexBufferHandle CreateVertexBuffer(BufferUsage usage, VertexInfo vInfo, int vertexCount)
        {
            uint id = ++_nextVertexBufferHandleID;
            VertexBufferHandle handle = new(id);

            var cmd = _createPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Usage = usage;
            cmd.VertexInfo = vInfo;
            cmd.VertexCount = vertexCount;

            RenderThread.Enqueue(cmd);

            return handle;
        }


        public VertexInfo GetVertexBufferLayout(VertexBufferHandle handle)
        {
            return _vertexBuffers[handle.Id].Layout;
        }


        public void SetVertexBufferData<T>(VertexBufferHandle handle, T[] data) where T : struct
        {
            var pool = GetSetDataPool<T>();
            var cmd = pool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Data = data;

            RenderThread.Enqueue(cmd);
        }


        public void BindVertexBuffer(VertexBufferHandle handle, int slot = 0, int offset = 0)
        {
            var cmd = _bindPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Slot = slot;
            cmd.Offset = offset;

            RenderThread.Enqueue(cmd);
        }

        public void BindVertexBuffers(params VertexBufferHandle[] handles)
        {
            var cmd = _bindMultiplePool.Get();

            cmd.Manager = this;
            cmd.Handles = handles;

            RenderThread.Enqueue(cmd);
        }


        public void DeleteVertexBuffer(VertexBufferHandle handle)
        {
            var cmd = _deletePool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.EnqueueDelayedDelete(cmd);
        }
    }
}