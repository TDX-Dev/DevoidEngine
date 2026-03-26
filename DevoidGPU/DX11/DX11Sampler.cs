using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11Sampler : ISampler
    {
        public SamplerDescription Description { get; private set; }

        private SamplerState native;
        private DeviceContext deviceContext;

        bool disposed;

        public DX11Sampler(Device device, DeviceContext context, SamplerDescription description)
        {
            deviceContext = context;

            this.Description = description;

            var dxDesc = new SamplerStateDescription
            {
                Filter = DX11StateMapper.ToDXFilter(description.MinFilter, description.MagFilter, description.MipFilter, description.MaxAnisotropy),
                AddressU = DX11StateMapper.ToDXWrap(description.WrapU),
                AddressV = DX11StateMapper.ToDXWrap(description.WrapV),
                AddressW = DX11StateMapper.ToDXWrap(description.WrapW),
                MaximumAnisotropy = description.MaxAnisotropy,
                MipLodBias = description.MipLODBias,
                MinimumLod = description.MinLOD,
                MaximumLod = description.MaxLOD,
            };


            native = new SamplerState(device, dxDesc);
        }

        public void Bind(int slot)
        {
            deviceContext.PixelShader.SetSampler(slot, native);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            native?.Dispose();
            native = null;

            disposed = true;

            GC.SuppressFinalize(this);
        }

        ~DX11Sampler()
        {
            Dispose();
        }
    }
}