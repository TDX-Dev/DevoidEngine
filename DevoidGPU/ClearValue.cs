using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public enum ClearValueFormat
    {
        Float,
        UInt
    }
    public readonly struct ClearValue
    {
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Z;
        public readonly uint W;
        public readonly ClearValueFormat Format;

        public ClearValue(uint x, uint y, uint z, uint w, ClearValueFormat format)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            Format = format;
        }

        public static ClearValue Float(float x, float y, float z, float w)
        {
            return new ClearValue(
                BitConverter.SingleToUInt32Bits(x),
                BitConverter.SingleToUInt32Bits(y),
                BitConverter.SingleToUInt32Bits(z),
                BitConverter.SingleToUInt32Bits(w),
                ClearValueFormat.Float
            );
        }

        public static ClearValue UInt(uint x, uint y, uint z, uint w)
        {
            return new ClearValue(x, y, z, w, ClearValueFormat.UInt);
        }
    }
}
