using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class IndexBuffer
    {
        public int Count { get; private set; }
        public IndexFormat Format { get; }

        private readonly IIndexBuffer gpuBuffer;
        public IIndexBuffer GPU => gpuBuffer;

        public IndexBuffer(
            IGraphicsDevice device,
            ReadOnlySpan<uint> data,
            BufferUsage usage = BufferUsage.Index,
            IndexFormat format = IndexFormat.UInt32
        )
        {
            Count = data.Length;
            Format = IndexFormat.UInt32;

            unsafe
            {
                fixed (uint* ptr = data)
                {
                    gpuBuffer = device.CreateIndexBuffer(new IndexBufferDescription
                    {
                        Size = (ulong)(data.Length * sizeof(uint)),
                        Format = Format,
                        Usage = usage,
                        InitialData = data.Length > 0 ? (IntPtr)ptr : IntPtr.Zero
                    });
                }
            }
        }

        public void Update(ReadOnlySpan<uint> data)
        {
            Count = data.Length;

            gpuBuffer.Update(data);
        }
    }
}
