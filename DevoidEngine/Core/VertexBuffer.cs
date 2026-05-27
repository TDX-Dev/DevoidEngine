using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class VertexBuffer<T> where T : unmanaged
    {
        public int Count { get; private set; }
        public int Stride { get; }
        public VertexInfo Layout { get; }

        private readonly IVertexBuffer gpuBuffer;

        public IVertexBuffer GPU => gpuBuffer;

        public VertexBuffer(
            IGraphicsDevice device,
            ReadOnlySpan<T> data,
            VertexInfo layout,
            ResourceUsage usage)
        {
            Layout = layout;
            Stride = layout.SizeInBytes;
            Count = data.Length;

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
            Count = data.Length;
            gpuBuffer.Update(data);
        }
    }
}
