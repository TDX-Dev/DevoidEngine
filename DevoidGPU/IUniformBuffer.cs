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
        ResourceUsage Usage { get; }

        void Update<T>(T data) where T : struct;
        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
