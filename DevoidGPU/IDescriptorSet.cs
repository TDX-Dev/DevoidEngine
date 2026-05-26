using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface IDescriptorSet
    {
        IDescriptorLayout Layout { get; }

        void SetUniformBuffer(uint binding, IUniformBuffer buffer);
        void SetTexture(uint binding, ITexture texture);
        void SetSampler(uint binding, ISampler sampler);

    }
}
