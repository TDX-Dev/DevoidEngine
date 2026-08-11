using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class VertexBuffer<T> : IDisposable where T : unmanaged
    {
        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public int Stride { get; }
        public VertexInfo Layout { get; }

        private IVertexBuffer gpuBuffer;

        public IVertexBuffer GPU => gpuBuffer;

        private readonly ResourceUsage usage;

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
            this.usage = usage;

            gpuBuffer = device.CreateVertexBuffer(new VertexBufferDescription
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

            gpuBuffer.Update(data);
        }

        public void Update(nint data, int len)
        {
            EnsureCapacity(len);

            Count = len;

            gpuBuffer.Update(data, len * Stride);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= Capacity)
                return;

            int newCapacity = Math.Max(required, Capacity * 2);

            gpuBuffer.Dispose();

            gpuBuffer = Engine.GraphicsDevice.CreateVertexBuffer(new VertexBufferDescription
            {
                Size = (ulong)(newCapacity * Stride),
                Layout = Layout,
                Slot = 0,
                Usage = usage
            });

            Capacity = newCapacity;
        }

        public void Dispose()
        {
            GPU.Dispose();
        }
    }
}
