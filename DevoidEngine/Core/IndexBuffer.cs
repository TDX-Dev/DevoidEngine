using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class IndexBuffer : IDisposable
    {
        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public IndexFormat Format { get; }

        private IIndexBuffer _gpuBuffer;
        public IIndexBuffer Gpu => _gpuBuffer;

        private int IndexSize =>
            Format switch
            {
                IndexFormat.UInt16 => sizeof(ushort),
                IndexFormat.UInt32 => sizeof(uint),
                _ => throw new NotSupportedException()
            };

        private readonly ResourceUsage _usage;

        public IndexBuffer(
            IGraphicsDevice device,
            int capacity,
            ResourceUsage usage = ResourceUsage.Default,
            IndexFormat format = IndexFormat.UInt32)
        {
            Count = 0;
            Capacity = capacity;
            Format = format;
            this._usage = usage;

            _gpuBuffer = device.CreateIndexBuffer(new IndexBufferDescription
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

            _gpuBuffer.Update(data);
        }

        public void Update(nint data, int len)
        {
            EnsureCapacity(len);

            Count = len;

            _gpuBuffer.Update(data, len * IndexSize);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= Capacity)
                return;

            int newCapacity = Math.Max(required, Capacity * 2);

            _gpuBuffer.Dispose();

            _gpuBuffer = Engine.GraphicsDevice.CreateIndexBuffer(new IndexBufferDescription
            {
                Size = (ulong)(newCapacity * sizeof(uint)),
                Format = Format,
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