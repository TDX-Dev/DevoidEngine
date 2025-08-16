﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public enum TextureFormat
    {
        Unknown,
        RGBA8_UNorm,
        RGBA8_UNorm_SRGB,
        BGRA8_UNorm,

        RG16_Float,
        RGBA16_Float,
        RGBA32_Float,
        
        Depth24_Stencil8,
        Depth32_Float
    }

}
