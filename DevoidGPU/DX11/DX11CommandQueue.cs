using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal sealed class DX11CommandQueue : ICommandQueue
    {
        public DX11CommandQueue(DeviceContext deviceContext) { }
        public void Submit(ICommandList commandList)
        {

        }
    }
}
