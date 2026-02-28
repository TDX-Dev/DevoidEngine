using DevoidEngine.Engine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class VertexBufferManager
    {
        private uint _nextVertexBufferHandleID = 0;
        private Dictionary<uint, IVertexBuffer> _vertexBuffers = new Dictionary<uint, IVertexBuffer>();

        public VertexBufferHandle CreateVertexBuffer(BufferUsage usage, VertexInfo vInfo, int vertexCount)
        {
            uint id = ++_nextVertexBufferHandleID;
            VertexBufferHandle resourceHandle = new VertexBufferHandle(id);

            RenderThread.Enqueue(() =>
            {

                _vertexBuffers[resourceHandle.Id]
                    = Renderer.graphicsDevice.BufferFactory.CreateVertexBuffer(
                        usage, vInfo, vertexCount
                    );
            });

            return resourceHandle;
        }

        public VertexInfo GetVertexBufferLayout(VertexBufferHandle handle)
        {
            return _vertexBuffers[handle.Id].Layout;
        }

        public void SetVertexBufferData<T>(VertexBufferHandle handle, T[] data) where T : struct
        {
            RenderThread.Enqueue(() =>
            {
                _vertexBuffers[handle.Id].SetData(data);
            });
        }

        public void BindVertexBuffer(VertexBufferHandle handle, int slot = 0, int offset = 0)
        {
            RenderThread.Enqueue(() =>
            {
                _vertexBuffers[handle.Id].Bind(slot, offset);
            });
        }

        public void DeleteVertexBuffer(VertexBufferHandle handle)
        {
            RenderThread.EnqueueDelayedDelete(() =>
            {
                if (_vertexBuffers.TryGetValue(handle.Id, out var vb))
                {
                    vb.Dispose();
                    _vertexBuffers.Remove(handle.Id);
                }
            });
        }

    }
}
