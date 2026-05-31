namespace DevoidGPU
{
    public interface IFrameBuffer : IDisposable
    {
        int Width { get; }
        int Height { get; }

        IReadOnlyList<ITexture> ColorAttachments { get; }
        ITexture? DepthAttachment { get; }
    }
}
