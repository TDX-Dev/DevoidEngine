namespace DevoidGPU
{
    public enum BlendMode
    {
        Opaque,         // No blending
        AlphaBlend,     // SrcAlpha, 1-SrcAlpha
        Additive,       // SrcAlpha, One
        Multiply        // DstColor, Zero
    }

}