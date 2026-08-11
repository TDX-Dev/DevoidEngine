using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class IndexBuffer : IDisposable
    {
        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public IndexFormat Format { get; }

        private IIndexBuffer gpuBuffer;
        public IIndexBuffer GPU => gpuBuffer;

        private int IndexSize =>
            Format switch
            {
                IndexFormat.UInt16 => sizeof(ushort),
                IndexFormat.UInt32 => sizeof(uint),
                _ => throw new NotSupportedException()
            };

        private readonly ResourceUsage usage;

        public IndexBuffer(
            IGraphicsDevice device,
            int capacity,
            ResourceUsage usage = ResourceUsage.Default,
            IndexFormat format = IndexFormat.UInt32)
        {
            Count = 0;
            Capacity = capacity;
            Format = format;
            this.usage = usage;

            gpuBuffer = device.CreateIndexBuffer(new IndexBufferDescription
            {
                Size = (ulong)(capacity * sizeof(uint)),
                Format = format,
                Usage = usage
            });
        }

        public IndexBuffer(
            IGraphicsDevice device,
            ReadOnlySpan<uint> data,
            ResourceUsage usage = ResourceUsage.Default,
            IndexFormat format = IndexFormat.UInt32)
            : this(device, data.Length, usage, format)
        {
            Update(data);
        }

        public void Update(ReadOnlySpan<uint> data)
        {
            EnsureCapacity(data.Length);

            Count = data.Length;

            gpuBuffer.Update(data);
        }

        public void Update(nint data, int len)
        {
            EnsureCapacity(len);

            Count = len;

            gpuBuffer.Update(data, len * IndexSize);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= Capacity)
                return;

            int newCapacity = Math.Max(required, Capacity * 2);

            gpuBuffer.Dispose();

            gpuBuffer = Engine.GraphicsDevice.CreateIndexBuffer(new IndexBufferDescription
            {
                Size = (ulong)(newCapacity * sizeof(uint)),
                Format = Format,
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