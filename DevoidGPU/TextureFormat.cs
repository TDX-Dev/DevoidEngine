namespace DevoidGPU
{
    public enum TextureFormat
    {
        Unknown,
        RGBA8UNorm,
        RGBA8UNormSrgb,
        BGRA8UNorm,

        RG16Float,
        RGBA16Float,
        RGBA32Float,

        R16Float,
        R32Float,

        R8UInt,
        R8UNorm,

        Depth24Stencil8,
        Depth32Float,

        // Add Formats at the end due to serialization logic
        RG8UNorm,
        R32UInt,

        R8SInt,
        R16SInt,
        R32SInt,
        RG8SInt,
        RG16SInt,
        RG32SInt,
        RGBA8SInt,
        RGBA16SInt,
        RGBA32SInt,
    }
}
