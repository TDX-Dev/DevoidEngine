using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct TextureSampleDescription
    {
        public int Count;
        public int Quality;

        public TextureSampleDescription(int count, int quality)
        {
            Count = count;
            Quality = quality;
        }
    }
}
