using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal sealed class DX11DescriptorSet : IDescriptorSet
    {

        public IDescriptorLayout Layout { get; }


        private readonly Dictionary<uint, IUniformBuffer> uniformBuffers = [];
        private readonly Dictionary<uint, ITexture> textures = [];
        private readonly Dictionary<uint, ISampler> samplers = [];

        public DX11DescriptorSet(IDescriptorLayout layout)
        {
            Layout = layout;
        }

        public void SetUniformBuffer(
            uint binding,
            IUniformBuffer buffer)
        {
            uniformBuffers[binding] = buffer;
        }

        public void SetTexture(
            uint binding,
            ITexture texture)
        {
            textures[binding] = texture;
        }

        public void SetSampler(
            uint binding,
            ISampler sampler)
        {
            samplers[binding] = sampler;
        }
    }
}
