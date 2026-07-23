using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface IShaderStorageBuffer : IDisposable
    {
        ulong Size { get; }
        ResourceUsage Usage { get; }
        void Update<T>(ReadOnlySpan<T> data) where T : unmanaged;
    }
}
