namespace DevoidGPU
{
    public struct TextureDescription
    {
        public int Width;
        public int Height;
        public TextureFormat Format;
        public TextureType Type;

        public int MipLevels;
        public bool GenerateMipmaps;
        public bool IsRenderTarget;
        public bool IsDepthStencil;
        public bool IsMutable;


    }
}
