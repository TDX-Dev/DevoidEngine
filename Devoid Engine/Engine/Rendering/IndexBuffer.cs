using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
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
            _indexBuffer = Renderer.ResourceManager.IndexBufferManager.CreateIndexBuffer(usage, indexCount);
        }

        public void Bind()
        {
            Renderer.ResourceManager.IndexBufferManager.BindIndexBuffer(_indexBuffer);
        }

        public void SetData(int[] indices)
        {
            Renderer.ResourceManager.IndexBufferManager.SetIndexBufferData(_indexBuffer, indices);
        }

        ~IndexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            Renderer.ResourceManager.IndexBufferManager.DeleteIndexBuffer(_indexBuffer);
            isDisposed = true;
            GC.SuppressFinalize(this);
        }

    }
}