using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class VertexBuffer<T> where T : unmanaged
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
            ReadOnlySpan<T> data,
            VertexInfo layout,
            ResourceUsage usage)
        {
            Layout = layout;
            Stride = layout.SizeInBytes;
            Count = data.Length;
            Capacity = data.Length;
            this.usage = usage;

            unsafe
            {
                fixed (T* ptr = data)
                {
                    gpuBuffer = device.CreateVertexBuffer(new VertexBufferDescription
                    {
                        Size = (ulong)(data.Length * sizeof(T)),
                        Layout = layout,
                        Slot = 0,
                        Usage = usage,
                        InitialData = data.Length > 0 ? (IntPtr)ptr : IntPtr.Zero
                    });
                }
            }
        }

        public void Update(ReadOnlySpan<T> data)
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

            gpuBuffer = Engine.GraphicsDevice.CreateVertexBuffer(new VertexBufferDescription
            {
                Size = (ulong)(newCapacity * Stride),
                Layout = Layout,
                Slot = 0,
                Usage = usage
            });

            Capacity = newCapacity;
        }
    }
}
