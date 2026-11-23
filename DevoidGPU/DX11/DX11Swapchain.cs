using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using SwapChain = SharpDX.DXGI.SwapChain;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11SwapChain : ISwapchain
    {
        public int Width { get; }
        public int Height { get; }
        public bool VSync { get; private set; }
        public IFrameBuffer Framebuffer => _framebuffer;

        private readonly Device _device;
        private readonly SwapChain _swapchain;
        private readonly DX11Texture[] _backbuffers;
        private readonly int _bufferCount;
        private readonly Format _format;
        private readonly SwapChainDescription _swapchainDescription;
        private Dx11Framebuffer _framebuffer = null!;

        public Dx11SwapChain(Factory factory, Device dx11Device, SwapchainDescription desc)
        {
            if (desc.WindowHandle == IntPtr.Zero)
                throw new ArgumentException("[DX11]: Window Handle provided was null.");

            _device = dx11Device;
            _bufferCount = desc.BufferCount;
            _format = Dx11StateMapper.ToDxgiTextureFormat(desc.Format);

            Width = desc.Width;
            Height = desc.Height;
            VSync = desc.VSync;

            _swapchainDescription = new SwapChainDescription()
            {
                BufferCount = _bufferCount,
                ModeDescription = new ModeDescription()
                {
                    Width = desc.Width,
                    Height = desc.Height,
                    RefreshRate = new Rational((int)desc.RefreshRate.X, (int)desc.RefreshRate.Y),

                    Format = _format
                },
                IsWindowed = desc.Windowed,
                Usage = Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.FlipDiscard,

                SampleDescription = new SampleDescription(desc.Samples.Count, desc.Samples.Quality),

                OutputHandle = desc.WindowHandle
            };

            var swapChain = new SwapChain(factory, _device, _swapchainDescription);

            _swapchain = swapChain;

            int count = _swapchain.Description.BufferCount;
            _backbuffers = new DX11Texture[count];

            CreateBackbuffer();

        }

        private void CreateBackbuffer()
        {
            using var tex = _swapchain.GetBackBuffer<Texture2D>(0);

            _backbuffers[0] = new DX11Texture(_device, tex);

            _framebuffer = new Dx11Framebuffer(
                [_backbuffers[0]]
            );
        }

        public void Present()
        {

            Result result = _swapchain.TryPresent(VSync ? 1 : 0, PresentFlags.None);

            if (result != Result.Ok)
            {
                Console.Write("[DX11]: GPU device problem! Reason: ");
                Console.WriteLine(_device.DeviceRemovedReason);
            }
        }

        public void Resize(int width, int height)
        {
            for (int i = 0; i < _backbuffers.Length; i++)
                _backbuffers[i]?.Dispose();

            foreach (var rtv in _framebuffer.RtVs)
            {
                rtv?.Dispose();
            }

            _swapchain.ResizeBuffers(
                _bufferCount,
                width,
                height,
                _format,
                SwapChainFlags.None
            );

            CreateBackbuffer();
        }

        public void Dispose()
        {
            for (int i = 0; i < _backbuffers.Length; i++)
                _backbuffers[i]?.Dispose();
            _swapchain.Dispose();
        }
    }
}
