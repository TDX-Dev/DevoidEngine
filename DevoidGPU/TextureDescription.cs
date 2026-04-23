using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct TextureDescription
    {
        public TextureDimension Dimension;
        public int Width;
        public int Height;
        public int Depth;

        public int MipLevels;
        public int ArraySize;
        public TextureFormat Format;

        public TextureUsage Usage;
        public TextureSampleDescription Samples;
    }
}
