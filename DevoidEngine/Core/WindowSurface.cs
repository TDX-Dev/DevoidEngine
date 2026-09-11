using DevoidEngine.InputSystem.InputDevices;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class WindowSurface : IDisposable
    {
        public Window Window { get; } = null!;
        public ISwapchain Swapchain { get; } = null!;
        public bool SkipRefresh => isMinimized;
        public IFrameBuffer Framebuffer => Swapchain.Framebuffer;

        public event Action<float>? OnUpdate;
        public event Action<ICommandList>? OnRender;

        public event Action<int, int>? OnResize;

        public event Action<char>? OnTextInput;
        public event Action<Keys, int, KeyModifiers, bool>? OnKeyDown;
        public event Action<Keys, int, KeyModifiers, bool>? OnKeyUp;

        private bool isMinimized;
        private bool resizePending;
        private bool isDisposed;
        private int prevWidth, prevHeight;

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
            Window.Minimized += Window_Minimized;
            window.OnWindowTextInput += Window_OnWindowTextInput;
            window.OnWindowKeyDown += Window_OnWindowKeyDown;
            window.OnWindowKeyUp += Window_OnWindowKeyUp;

            prevWidth = Window.ClientSize.X;
            prevHeight = Window.ClientSize.Y;
        }

        private void Window_OnWindowKeyUp(OpenTK.Windowing.GraphicsLibraryFramework.Keys arg1, int arg2, OpenTK.Windowing.GraphicsLibraryFramework.KeyModifiers arg3, bool arg4)
        {
            OnKeyUp?.Invoke((Keys)arg1, arg2, (KeyModifiers)arg3, arg4);
        }

        private void Window_OnWindowKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys arg1, int arg2, OpenTK.Windowing.GraphicsLibraryFramework.KeyModifiers arg3, bool arg4)
        {
            OnKeyDown?.Invoke((Keys)arg1, arg2, (KeyModifiers)arg3, arg4);
        }

        private void Window_OnWindowTextInput(char obj)
        {
            OnTextInput?.Invoke(obj);
        }

        private void Window_Minimized(OpenTK.Windowing.Common.MinimizedEventArgs obj)
        {
            isMinimized = obj.IsMinimized;
        }

        private void Window_Resize(int width, int height)
        {
            if (width <= 0 || height <= 0 || (height == prevHeight && width == prevWidth))
                return;
            resizePending = true;
        }

        private void ResizeSwapchain()
        {
            if (!resizePending || isMinimized)
                return;
            Swapchain.Resize(Window.ClientSize.X, Window.ClientSize.Y);
            prevWidth = Window.ClientSize.X;
            prevHeight = Window.ClientSize.Y;

            OnResize?.Invoke(Window.ClientSize.X, Window.ClientSize.Y);
            resizePending = false;
        }

        public void UpdateSurface(float deltaTime)
        {
            OnUpdate?.Invoke(deltaTime);
        }

        public void RenderSurface(ICommandList cmd)
        {


            OnRender?.Invoke(cmd);
        }

        public void Present()
        {
            Swapchain.Present();
            ResizeSwapchain();
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
