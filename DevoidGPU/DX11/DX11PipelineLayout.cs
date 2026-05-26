using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal sealed class DX11PipelineLayout : IPipelineLayout
    {
        public IReadOnlyList<IDescriptorLayout> SetLayouts { get; }

        public DX11PipelineLayout(IDescriptorLayout[] setLayouts)
        {
            SetLayouts = setLayouts;
        }
    }
}
