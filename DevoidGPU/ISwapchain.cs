namespace DevoidGPU
{
    public interface ISwapchain
    {
        int Width { get; }
        int Height { get; }

        void Present();
        void Resize(int width, int height);
        void Dispose();
    }
}
