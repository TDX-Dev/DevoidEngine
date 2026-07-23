using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class UniformBuffer : IDisposable
    {
        private readonly IUniformBuffer gpuBuffer;
        public IUniformBuffer GPU => gpuBuffer;

        public UniformBuffer(
            IGraphicsDevice device,
            ResourceUsage usage,
            uint size
        )
        {
            gpuBuffer = device.CreateUniformBuffer(new BufferDescription()
            {
                Usage = usage,
                Bind = BufferBind.Uniform,
                CpuAccess = CpuAccess.Write,
                InitialData = IntPtr.Zero,
                Size = size
            });
        }

        public static UniformBuffer Create(ResourceUsage usage, uint size)
        {
            return new UniformBuffer(Engine.GraphicsDevice, usage, size);
        }

        public void Update<T>(T data) where T : struct
        {
            GPU.Update(data);
        }

        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            GPU.Update<T>(data);
        }

        public void Update(ReadOnlySpan<byte> data)
        {
            GPU.Update(data);
        }

        public void Dispose()
        {
            GPU.Dispose();
        }
    }
}
