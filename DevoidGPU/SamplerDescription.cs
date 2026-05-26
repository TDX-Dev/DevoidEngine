using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct SamplerDescription
    {
        public FilterMode MinFilter;
        public FilterMode MagFilter;
        public FilterMode MipFilter;

        public WrapMode AddressU;
        public WrapMode AddressV;
        public WrapMode AddressW;

        public CompareFunc CompareFunc;

        public float MipLODBias;
        public float MinLOD;
        public float MaxLOD;

        public int MaxAnisotropy;
    }
}
