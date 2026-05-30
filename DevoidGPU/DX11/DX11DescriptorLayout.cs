using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal sealed class DX11DescriptorLayout : IDescriptorLayout
    {
        public DescriptorBinding[] Bindings { get; }

        public DX11DescriptorLayout(DescriptorBinding[] bindings)
        {
            Bindings = bindings;
        }
    }
}
