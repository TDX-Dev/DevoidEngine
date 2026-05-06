namespace DevoidGPU
{
    public interface IIndexBuffer : IDisposable
    {
        ulong Size { get; }
        IndexFormat Format { get; }
        BufferUsage Usage { get; }

        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
