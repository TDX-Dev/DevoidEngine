using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using SwapChain = SharpDX.DXGI.SwapChain;

namespace DevoidGPU.DX11
{
    class DX11SwapChain : ISwapchain
    {
        public int Width { get; }
        public int Height { get; }
        public bool VSync { get; private set; }

        private readonly Device device;
        private readonly SwapChain swapchain;
        private readonly DX11Texture[] backbuffers;
        private readonly int bufferCount;
        private readonly Format format;
        private readonly SwapChainDescription swapchainDescription;

        public DX11SwapChain(Factory factory, Device dx11Device, SwapchainDescription desc)
        {
            if (desc.WindowHandle == IntPtr.Zero)
                throw new ArgumentException("[DX11]: Window Handle provided was null.");

            device = dx11Device;
            bufferCount = desc.BufferCount;
            format = DX11StateMapper.ToDXGITextureFormat(desc.Format);

            Width = desc.Width;
            Height = desc.Height;
            VSync = desc.VSync;

            swapchainDescription = new SwapChainDescription()
            {
                BufferCount = bufferCount,
                ModeDescription = new ModeDescription()
                {
                    Width = desc.Width,
                    Height = desc.Height,
                    RefreshRate = new Rational((int)desc.RefreshRate.X, (int)desc.RefreshRate.Y),

                    Format = format
                },
                IsWindowed = desc.Windowed,
                Usage = Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.FlipDiscard,

                SampleDescription = new SampleDescription(desc.Samples.Count, desc.Samples.Quality),

                OutputHandle = desc.WindowHandle
            };

            var swapChain = new SwapChain(factory, device, swapchainDescription);

            swapchain = swapChain;

            int count = swapchain.Description.BufferCount;
            backbuffers = new DX11Texture[count];

            CreateBackbuffer();

        }

        private void CreateBackbuffer()
        {
            using var tex = swapchain.GetBackBuffer<Texture2D>(0);
            backbuffers[0] = new DX11Texture(device, tex);
        }

        public void Present()
        {
            Result result = swapchain.TryPresent(VSync ? 1 : 0, PresentFlags.None);

            if (result != Result.Ok)
            {
                Console.WriteLine("[DX11]: GPU device problem! Reason: " + device.DeviceRemovedReason);
            }
        }

        public void Resize(int width, int height)
        {
            for (int i = 0; i < backbuffers.Length; i++)
                backbuffers[i]?.Dispose();

            swapchain.ResizeBuffers(
                bufferCount,
                width,
                height,
                format,
                SwapChainFlags.None
            );

            CreateBackbuffer();
        }

        public void Dispose()
        {
            for (int i = 0; i < backbuffers.Length; i++)
                backbuffers[i]?.Dispose();
            swapchain.Dispose();
        }
    }
}
