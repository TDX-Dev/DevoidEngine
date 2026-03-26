namespace DevoidGPU
{
    public interface IVertexBuffer
    {
        int VertexCount { get; }
        int Stride { get; }
        VertexInfo Layout { get; }
        BufferUsage Usage { get; }

        void SetData<T>(T[] vertices, int startVertex = 0) where T : struct;
        void UpdatePartial<T>(T[] vertices, int startVertex, int vertexCount) where T : struct;
        void UpdatePartial(nint dataPtr, int startVertex, int vertexCount);
        void Bind(int slot = 0, int offset = 0);
        void Dispose();
    }
}