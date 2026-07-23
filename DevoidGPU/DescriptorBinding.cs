using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct DescriptorBinding
    {
        public uint Binding;
        public DescriptorType Type;
        public ShaderStage Stages;

        public override readonly string ToString()
        {
            return $"Binding={Binding}, Type={Type}, Stages={Stages}";
        }
    }
}
