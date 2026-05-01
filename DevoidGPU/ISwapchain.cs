namespace DevoidGPU
{
    public interface ISwapchain : IDisposable
    {
        int Width { get; }
        int Height { get; }

        void Present();
        void Resize(int width, int height);
    }
}
