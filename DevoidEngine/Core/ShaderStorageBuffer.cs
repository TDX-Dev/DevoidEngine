using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class ShaderStorageBuffer<T> : IDisposable
    where T : unmanaged
    {
        private readonly IShaderStorageBuffer gpuBuffer;

        public IShaderStorageBuffer GPU => gpuBuffer;

        public ulong Size => gpuBuffer.Size;
        public ResourceUsage Usage => gpuBuffer.Usage;

        public ShaderStorageBuffer(
            IGraphicsDevice device,
            ResourceUsage usage,
            uint capacity)
        {
            gpuBuffer = device.CreateShaderStorageBuffer(
                new BufferDescription
                {
                    Usage = usage,
                    Bind = BufferBind.Storage,
                    CpuAccess = CpuAccess.Write,
                    InitialData = IntPtr.Zero,
                    Size = capacity * (uint)Unsafe.SizeOf<T>(),
                    Stride = Unsafe.SizeOf<T>()
                });
        }

        public static ShaderStorageBuffer<T> Create(
            ResourceUsage usage,
            uint capacity)
        {
            return new(
                Engine.GraphicsDevice,
                usage,
                capacity);
        }

        public void Update(ReadOnlySpan<T> data)
        {
            gpuBuffer.Update(data);
        }

        public void Dispose()
        {
            gpuBuffer.Dispose();
        }
    }
}
