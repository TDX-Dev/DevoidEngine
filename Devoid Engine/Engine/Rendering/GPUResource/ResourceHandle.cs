using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public readonly struct VertexBufferHandle
    {
        public readonly uint Id;

        internal VertexBufferHandle(uint id)
        {
            Id = id;
        }
    }

    public readonly struct SamplerHandle
    {
        public readonly uint Id;

        internal SamplerHandle(uint id)
        {
            Id = id;
        }
    }

    public readonly struct TextureHandle
    {
        public readonly uint Id;

        internal TextureHandle(uint id)
        {
            Id = id;
        }
    }
}
