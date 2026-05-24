using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface IUniformBuffer
    {
        ulong Size { get; }
        BufferUsage Usage { get; }
        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
