using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class VertexBuffer<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public int Stride { get; }
        public VertexInfo Layout { get; }

        private IVertexBuffer _gpuBuffer;

        public IVertexBuffer Gpu => _gpuBuffer;

        private readonly ResourceUsage _usage;

        public VertexBuffer(
            IGraphicsDevice device,
            int capacity,
            VertexInfo layout,
            ResourceUsage usage)
        {
            Layout = layout;
            Stride = layout.SizeInBytes;
            Count = 0;
            Capacity = capacity;
            this._usage = usage;

            _gpuBuffer = device.CreateVertexBuffer(new VertexBufferDescription
            {
                Size = (ulong)(capacity * Stride),
                Layout = layout,
                Slot = 0,
                Usage = usage
            });
        }

        public VertexBuffer(
            IGraphicsDevice device,
            ReadOnlySpan<T> data,
            VertexInfo layout,
            ResourceUsage usage)
            : this(device, data.Length, layout, usage)
        {
            Update(data);
        }

        public void Update(ReadOnlySpan<T> data)
        {
            EnsureCapacity(data.Length);

            Count = data.Length;

            _gpuBuffer.Update(data);
        }

        public void Update(nint data, int len)
        {
            EnsureCapacity(len);

            Count = len;

            _gpuBuffer.Update(data, len * Stride);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= Capacity)
                return;

            int newCapacity = Math.Max(required, Capacity * 2);

            _gpuBuffer.Dispose();

            _gpuBuffer = Engine.GraphicsDevice.CreateVertexBuffer(new VertexBufferDescription
            {
                Size = (ulong)(newCapacity * Stride),
                Layout = Layout,
                Slot = 0,
                Usage = _usage
            });

            Capacity = newCapacity;
        }

        public void Dispose()
        {
            Gpu.Dispose();
        }
    }
}
