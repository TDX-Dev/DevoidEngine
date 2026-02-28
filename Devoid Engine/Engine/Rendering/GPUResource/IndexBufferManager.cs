using DevoidEngine.Engine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class IndexBufferManager
    {
        public uint _nextIndexBufferHandleID = 0;
        private Dictionary<uint, IIndexBuffer> _indexBuffers = new Dictionary<uint, IIndexBuffer>();

        public IndexBufferHandle CreateIndexBuffer(BufferUsage usage, int indexCount)
        {
            uint id = ++_nextIndexBufferHandleID;
            IndexBufferHandle resourceHandle = new IndexBufferHandle(id);

            RenderThread.Enqueue(() =>
            {

                _indexBuffers[resourceHandle.Id]
                    = Renderer.graphicsDevice.BufferFactory.CreateIndexBuffer(
                        indexCount, usage
                    );
            });

            return resourceHandle;
        }

        public void BindIndexBuffer(IndexBufferHandle handle)
        {
            RenderThread.Enqueue(() =>
            {
                _indexBuffers[handle.Id].Bind();
            });
        }

        public void SetIndexBufferData(IndexBufferHandle handle, int[] data)
        {
            RenderThread.Enqueue(() =>
            {
                _indexBuffers[handle.Id].SetData(data);
            });
        }
        public void DeleteIndexBuffer(IndexBufferHandle handle)
        {
            RenderThread.EnqueueDelayedDelete(() =>
            {
                if (_indexBuffers.TryGetValue(handle.Id, out var ib))
                {
                    ib.Dispose();
                    _indexBuffers.Remove(handle.Id);
                }
            });
        }

    }
}
