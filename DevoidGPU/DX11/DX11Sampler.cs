using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class DX11Sampler : ISampler
    {

        internal SamplerState Sampler;

        private readonly Device device;

        public DX11Sampler(Device device, SamplerDescription description)
        {
            this.device = device;

            SamplerStateDescription dxDescription = new()
            {
                AddressU = DX11StateMapper.ToDXTextureAddressMode(description.AddressU),
                AddressV = DX11StateMapper.ToDXTextureAddressMode(description.AddressV),
                AddressW = DX11StateMapper.ToDXTextureAddressMode(description.AddressW),
                MipLodBias = description.MipLODBias,
                MaximumLod = description.MaxLOD,
                MinimumLod = description.MinLOD,
                ComparisonFunction = DX11StateMapper.ToDXDepthComparison(description.CompareFunc),
                MaximumAnisotropy = description.MaxAnisotropy
            };

            Sampler = new SamplerState(device, dxDescription);

        }

        public void Dispose()
        {
            Sampler.Dispose();
        }
    }
}
