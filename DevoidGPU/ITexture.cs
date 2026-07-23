namespace DevoidGPU
{
    public interface ITexture : IDisposable
    {
        TextureDescription Description { get; }
        int Width { get; }
        int Height { get; }

        TextureFormat Format { get; }

        void Update(ReadOnlySpan<byte> data);

        void Update<T>(ReadOnlySpan<T> data)
            where T : unmanaged;
    }
}
