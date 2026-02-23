namespace DevoidGPU
{
    public interface ITextureFactory
    {
        ITexture CreateTexture(TextureDescription description);
        ITexture2D CreateTexture2D(TextureDescription description);
        ITexture3D CreateTexture3D();
        ITextureCube CreateTextureCube();


    }
}
