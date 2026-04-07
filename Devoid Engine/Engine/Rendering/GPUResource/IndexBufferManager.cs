using DevoidEngine.Engine.Core;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class IndexBufferManager
    {
        public uint _nextIndexBufferHandleID = 0;

        internal Dictionary<uint, IIndexBuffer> _indexBuffers = new();

        private RenderCommandPool<CreateIndexBufferCommand> _createPool = new();
        private RenderCommandPool<BindIndexBufferCommand> _bindPool = new();
        private RenderCommandPool<SetIndexBufferDataCommand> _setDataPool = new();
        private RenderCommandPool<DeleteIndexBufferCommand> _deletePool = new();


        public IndexBufferHandle CreateIndexBuffer(BufferUsage usage, int indexCount)
        {
            uint id = ++_nextIndexBufferHandleID;
            IndexBufferHandle handle = new(id);

            var cmd = _createPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Usage = usage;
            cmd.IndexCount = indexCount;

            RenderThread.Enqueue(cmd);

            return handle;
        }


        public void BindIndexBuffer(IndexBufferHandle handle)
        {
            var cmd = _bindPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);
        }


        public void SetIndexBufferData(IndexBufferHandle handle, int[] data)
        {
            var cmd = _setDataPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Data = data;

            RenderThread.Enqueue(cmd);
        }


        public void DeleteIndexBuffer(IndexBufferHandle handle)
        {
            var cmd = _deletePool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.EnqueueDelayedDelete(cmd);
        }
    }
}