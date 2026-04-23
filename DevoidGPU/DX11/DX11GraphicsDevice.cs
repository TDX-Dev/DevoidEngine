using SharpDX.Direct3D;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using DeviceCreationFlags = SharpDX.Direct3D11.DeviceCreationFlags;
using DriverType = SharpDX.Direct3D.DriverType;
using SampleDescription = SharpDX.DXGI.SampleDescription;

namespace DevoidGPU.DX11
{
    public class DX11GraphicsDevice : IGraphicsDevice
    {
        private readonly Factory1 factory = null!;
        private readonly Device device = null!;
        private readonly DeviceContext deviceContext = null!;

        public DX11GraphicsDevice()
        {
            factory = new Factory1();

            var levels = new[]
            {
                FeatureLevel.Level_11_0,
            };


            device = new Device(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport |
                #if DEBUG
                DeviceCreationFlags.Debug
                #endif
                , levels
            );

            deviceContext = device.ImmediateContext;
        }

        public ISwapchain CreateSwapchain(SwapchainDescription desc)
        {

            return new DX11Swapchain(factory, device, desc);
        }
    }
}
