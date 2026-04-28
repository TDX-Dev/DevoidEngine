namespace DevoidGPU
{
    public interface IVertexBuffer : IDisposable
    {
        ulong Size { get; }
        VertexInfo Layout { get; }
        int Slot { get; }
        int Stride { get; }
    }
}
