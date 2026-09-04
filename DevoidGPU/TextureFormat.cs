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

        R16_Float,
        R32_Float,

        R8_UInt,
        R8_UNorm,

        Depth24_Stencil8,
        Depth32_Float,

        // Add Formats at the end due to serialization logic
        RG8_UNorm,
        R32_UInt,

        R8_SInt,
        R16_SInt,
        R32_SInt,
        RG8_SInt,
        RG16_SInt,
        RG32_SInt,
        RGBA8_SInt,
        RGBA16_SInt,
        RGBA32_SInt,
    }
}
