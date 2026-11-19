using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class UniformBuffer : IDisposable
    {
        private readonly IUniformBuffer _gpuBuffer;
        public IUniformBuffer Gpu => _gpuBuffer;

        public UniformBuffer(
            IGraphicsDevice device,
            ResourceUsage usage,
            uint size
        )
        {
            _gpuBuffer = device.CreateUniformBuffer(new BufferDescription()
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
            Gpu.Update(data);
        }

        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            Gpu.Update<T>(data);
        }

        public void Update(ReadOnlySpan<byte> data)
        {
            Gpu.Update(data);
        }

        public void Dispose()
        {
            Gpu.Dispose();
        }
    }
}
