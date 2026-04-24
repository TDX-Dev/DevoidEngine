using DevoidGPU;
using OpenTK.Windowing.Common;
using SharpDX.DXGI;

namespace DevoidEngine.Core
{
    public sealed class WindowSurface : IDisposable
    {
        public Window Window { get; } = null!;
        public ISwapchain Swapchain { get; } = null!;

        private bool isDisposed;

        public WindowSurface(
            Window window,
            IGraphicsDevice device,
            SwapchainDescription desc
        )
        {
            Window = window;

            desc.WindowHandle = window.Handle;

            Swapchain = device.CreateSwapchain(desc);
            Window.OnWindowResize += Window_Resize;
        }

        private void Window_Resize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return;

            Swapchain.Resize(width, height);
        }

        public void Present()
        {
            if (Window.ClientSize.X == 0 || Window.ClientSize.Y == 0)
                return;
            Swapchain.Present();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            Window.OnWindowResize -= Window_Resize;

            Swapchain.Dispose();
            Window.Dispose();

            isDisposed = true;
        }
    }
}
