namespace DevoidGPU
{
    public interface ITexture : IDisposable
    {
        TextureDescription Description { get; }
        int Width { get; }
        int Height { get; }

        TextureFormat Format { get; }
    }
}
