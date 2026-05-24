using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class UniformBuffer
    {
        private readonly IUniformBuffer gpuBuffer;
        public IUniformBuffer GPU => gpuBuffer;

        public UniformBuffer(
            IGraphicsDevice device,
            BufferUsage usage,
            uint size
        )
        {
            gpuBuffer = device.CreateUniformBuffer(new UniformBufferDescription()
            {
                Usage = usage,
                InitialData = IntPtr.Zero,
                Size = size
            });
        }
    }
}
