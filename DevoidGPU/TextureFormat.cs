namespace DevoidGPU
{
    public enum TextureFormat
    {
        Unknown,
        Rgba8UNorm,
        Rgba8UNormSrgb,
        Bgra8UNorm,

        Rg16Float,
        Rgba16Float,
        Rgba32Float,

        R16Float,
        R32Float,

        R8UInt,
        R8UNorm,

        Depth24Stencil8,
        Depth32Float,

        // Add Formats at the end due to serialization logic
        Rg8UNorm,
        R32UInt,

        R8SInt,
        R16SInt,
        R32SInt,
        Rg8SInt,
        Rg16SInt,
        Rg32SInt,
        Rgba8SInt,
        Rgba16SInt,
        Rgba32SInt,
    }
}
