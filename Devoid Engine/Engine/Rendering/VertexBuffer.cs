using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering
{
    public class VertexBuffer : IDisposable
    {
        VertexBufferHandle _vertexBuffer;

        public int VertexCount { get; private set; } = 0;

        private bool isDisposed = false;

        public VertexBuffer(
            BufferUsage usage,
            VertexInfo vInfo,
            int vertexCount
        )
        {
            VertexCount = vertexCount;
            _vertexBuffer = Renderer.ResourceManager.VertexBufferManager.CreateVertexBuffer(usage, vInfo, vertexCount);
        }

        public void Bind()
        {
            Renderer.ResourceManager.VertexBufferManager.BindVertexBuffer(_vertexBuffer);
        }

        public VertexInfo GetVertexInfo()
        {
            return Renderer.ResourceManager.VertexBufferManager.GetVertexBufferLayout(_vertexBuffer);
        }

        public void SetData<T>(T[] vertices) where T : struct
        {
            Renderer.ResourceManager.VertexBufferManager.SetVertexBufferData(_vertexBuffer, vertices);
        }

        ~VertexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            Renderer.ResourceManager.VertexBufferManager.DeleteVertexBuffer(_vertexBuffer);
            isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}