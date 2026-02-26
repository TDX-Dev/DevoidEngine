using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Utilities
{
    public class IndexBuffer : IDisposable
    {
        IndexBufferHandle _indexBuffer;

        public int IndexCount { get; private set; } = 0;

        private bool isDisposed = false;

        public IndexBuffer(
            BufferUsage usage,
            int indexCount
        )
        {
            IndexCount = indexCount;
            _indexBuffer = Graphics.ResourceManager.IndexBufferManager.CreateIndexBuffer(usage, indexCount);
        }

        public void Bind()
        {
            Graphics.ResourceManager.IndexBufferManager.BindIndexBuffer(_indexBuffer);
        }

        public void SetData(int[] indices)
        {
            Graphics.ResourceManager.IndexBufferManager.SetIndexBufferData(_indexBuffer, indices);
        }

        ~IndexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            Graphics.ResourceManager.IndexBufferManager.DeleteIndexBuffer(_indexBuffer);
            isDisposed = true;
            GC.SuppressFinalize(this);
        }

    }
}
