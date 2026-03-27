using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal interface IDX11Texture : ITexture
    {
        ShaderResourceView ShaderResourceView { get; }
        UnorderedAccessView UnorderedAccessView { get; }
    }
}
