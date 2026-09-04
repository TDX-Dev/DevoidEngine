namespace DevoidGPU
{
    public interface ITexture : IDisposable
    {
        TextureDescription Description { get; }
        int Width { get; }
        int Height { get; }
        TextureDimension Dimension { get; }

        int MipLevels { get; }

        int ArraySize { get; }

        TextureFormat Format { get; }

        void Update(ReadOnlySpan<byte> data);

        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
