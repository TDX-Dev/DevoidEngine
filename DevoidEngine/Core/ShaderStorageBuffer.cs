using DevoidGPU;
using System.Runtime.CompilerServices;

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
            BufferBind bind,
            uint capacity)
        {
            gpuBuffer = device.CreateShaderStorageBuffer(
                new BufferDescription
                {
                    Usage = usage,
                    Bind = bind,
                    CpuAccess = usage.HasFlag(ResourceUsage.Dynamic)
                        ? CpuAccess.Write
                        : CpuAccess.None,
                    InitialData = IntPtr.Zero,
                    Size = capacity * (uint)Unsafe.SizeOf<T>(),
                    Stride = Unsafe.SizeOf<T>()
                });
        }

        public static ShaderStorageBuffer<T> Create(
            ResourceUsage usage,
            uint capacity,
            BufferBind bind = BufferBind.Storage)
        {
            return new(
                Engine.GraphicsDevice,
                usage,
                bind,
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
