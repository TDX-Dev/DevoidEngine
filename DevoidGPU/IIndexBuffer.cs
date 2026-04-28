namespace DevoidGPU
{
    public interface IIndexBuffer
    {
        ulong Size { get; }
        IndexFormat Format { get; }
    }
}
