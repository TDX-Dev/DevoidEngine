using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class IndexBuffer
    {
        public int Count { get; private set; }
        public int Capacity { get; private set; }
        public IndexFormat Format { get; }

        private IIndexBuffer gpuBuffer;
        public IIndexBuffer GPU => gpuBuffer;

        private readonly ResourceUsage usage;

        public IndexBuffer(
            IGraphicsDevice device,
            ReadOnlySpan<uint> data,
            ResourceUsage usage = ResourceUsage.Default,
            IndexFormat format = IndexFormat.UInt32
        )
        {
            Count = data.Length;
            Capacity = data.Length;
            Format = IndexFormat.UInt32;
            this.usage = usage;

            unsafe
            {
                fixed (uint* ptr = data)
                {
                    gpuBuffer = device.CreateIndexBuffer(new IndexBufferDescription
                    {
                        Size = (ulong)(Capacity * sizeof(uint)),
                        Format = Format,
                        Usage = usage,
                        InitialData = data.Length > 0 ? (IntPtr)ptr : IntPtr.Zero
                    });
                }
            }
        }

        public void Update(ReadOnlySpan<uint> data)
        {
            EnsureCapacity(data.Length);

            Count = data.Length;

            gpuBuffer.Update(data);
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
    }
}
