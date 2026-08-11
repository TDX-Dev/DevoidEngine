namespace DevoidGPU
{
    public interface IIndexBuffer : IDisposable
    {
        ulong Size { get; }
        IndexFormat Format { get; }
        ResourceUsage Usage { get; }
        void Update(nint data, int sizeInBytes);
        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
