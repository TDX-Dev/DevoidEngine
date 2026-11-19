using SharpDX.Direct3D11;
using System.Diagnostics;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11Sampler : ISampler
    {

        internal SamplerState Sampler;

        private readonly Device _device;

        public Dx11Sampler(Device device, SamplerDescription description)
        {
            this._device = device;

            SamplerStateDescription dxDescription = new()
            {
                AddressU = Dx11StateMapper.ToDxTextureAddressMode(description.AddressU),
                AddressV = Dx11StateMapper.ToDxTextureAddressMode(description.AddressV),
                AddressW = Dx11StateMapper.ToDxTextureAddressMode(description.AddressW),
                MipLodBias = description.MipLodBias,
                MaximumLod = description.MaxLod,
                MinimumLod = description.MinLod,
                ComparisonFunction = Dx11StateMapper.ToDxDepthComparison(description.CompareFunc),
                MaximumAnisotropy = description.MaxAnisotropy,
                Filter = Dx11StateMapper.ToDxFilter(description.MinFilter, description.MagFilter, description.MipFilter, description.MaxAnisotropy)
            };

            Sampler = new SamplerState(device, dxDescription);

        }

        public void Dispose()
        {
            Sampler.Dispose();
        }
    }
}
