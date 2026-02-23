namespace DevoidGPU
{
    public enum TextureType
    {
        Texture2D,
        Texture3D,
        TextureCube,
    }

    public interface ITexture : IDisposable
    {
        TextureType Type { get; }
        int Width { get; }
        int Height { get; }

        public bool IsRenderTarget { get; }
        public bool IsDepthStencil { get; }
        public IntPtr GetHandle();
        public void Bind(int slot = 0);
        public void UnBind(int slot = 0);
        public void BindMutable(int slot = 0);
        public void GenerateMipmaps();
    }

    public interface ITexture2D : ITexture
    {
        bool AllowUnorderedView { get; }
        void SetData(byte[] data);
        void SetData<T>(T[] data) where T : unmanaged;
    }
    public interface ITexture3D : ITexture { }
    public interface ITextureCube : ITexture { }
}
