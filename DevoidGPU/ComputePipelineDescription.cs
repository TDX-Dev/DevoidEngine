using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct ComputePipelineDescription
    {
        public IShader ComputeShader;

        public IPipelineLayout PipelineLayout;
    }
}
