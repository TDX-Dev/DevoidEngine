namespace DevoidGPU
{
    public interface IBufferFactory
    {
        IUniformBuffer CreateUniformBuffer<T>(BufferUsage bufferUsage) where T : struct;
        IUniformBuffer CreateUniformBuffer(int size, BufferUsage bufferUsage);
        IShaderStorageBuffer CreateShaderStorageBuffer(int size, int stride, BufferUsage usage, bool allowUAV = false);

        IVertexBuffer CreateVertexBuffer(BufferUsage bufferUsage, VertexInfo vertexInfo, int vertexCount);
        IIndexBuffer CreateIndexBuffer(int indexCount, BufferUsage usage, bool is16Bit = false);

        IFramebuffer CreateFramebuffer();
    }
}