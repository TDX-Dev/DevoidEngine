using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal sealed class DX11ComputePipeline : IComputePipeline
    {
        public IShader ComputeShader { get; }

        internal DX11Shader Shader => (DX11Shader)ComputeShader;

        public DX11ComputePipeline(ComputePipelineDescription desc)
        {
            if (desc.ComputeShader.Stage != ShaderStage.Compute)
                throw new ArgumentException(
                    "Expected a compute shader.");

            ComputeShader = desc.ComputeShader;
        }

        public void Dispose()
        {

        }
    }
}