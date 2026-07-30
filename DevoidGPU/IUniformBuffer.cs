namespace DevoidGPU
{
    public interface IUniformBuffer : IDisposable
    {
        ulong Size { get; }
        ResourceUsage Usage { get; }

        void Update<T>(T data) where T : struct;
        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
