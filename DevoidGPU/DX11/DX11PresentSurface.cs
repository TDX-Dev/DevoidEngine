using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    class DX11PresentSurface : IPresentSurface
    {
        private SwapChain swapChain;
        private RenderTargetView backBufferView;
        private SharpDX.Direct3D11.Device device;
        private SharpDX.Direct3D11.DeviceContext deviceContext;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool VSync { get; set; }

        public DX11PresentSurface(SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.DeviceContext deviceContext, SwapChain swapChain, RenderTargetView backBufferView)
        {
            this.device = device;
            this.deviceContext = deviceContext;
            this.swapChain = swapChain;
            this.backBufferView = backBufferView;

            var desc = swapChain.Description;
            Width = desc.ModeDescription.Width;
            Height = desc.ModeDescription.Height;
        }

        public void Resize(int width, int height)
        {
            backBufferView.Dispose();
            swapChain.ResizeBuffers(0, width, height, Format.Unknown, SwapChainFlags.None);
            using var backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
            backBufferView = new RenderTargetView(device, backBuffer);
            Width = width;
            Height = height;
        }

        public void Bind()
        {
            deviceContext.OutputMerger.SetRenderTargets(backBufferView);
        }

        public void Present()
        {
            //swapChain.Present(VSync ? 1 : 0, PresentFlags.None);
            try
            {
                swapChain.Present(VSync ? 1 : 0, PresentFlags.None);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode != SharpDX.Result.Ok)
                {
                    Console.WriteLine("GPU device problem! Reason: " + device.DeviceRemovedReason);
                }
                else
                {
                    throw;
                }
            }


        }

        public void ClearColor(System.Numerics.Vector4 color)
        {
            device.ImmediateContext.ClearRenderTargetView(backBufferView, new SharpDX.Mathematics.Interop.RawColor4(color.X, color.Y, color.Z, color.W));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}