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
            _vertexBuffer = Graphics.ResourceManager.VertexBufferManager.CreateVertexBuffer(usage, vInfo, vertexCount);
        }

        public void Bind()
        {
            Graphics.ResourceManager.VertexBufferManager.BindVertexBuffer( _vertexBuffer );
        }

        public VertexInfo GetVertexInfo()
        {
            return Graphics.ResourceManager.VertexBufferManager.GetVertexBufferLayout(_vertexBuffer);
        }

        public void SetData<T>(T[] vertices) where T : struct
        {
            Graphics.ResourceManager.VertexBufferManager.SetVertexBufferData(_vertexBuffer, vertices);
        }

        ~VertexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            Graphics.ResourceManager.VertexBufferManager.DeleteVertexBuffer(_vertexBuffer);
            isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
