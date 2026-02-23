using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    class DX11TextureFactory : ITextureFactory
    {
        private readonly Device device;
        private readonly DeviceContext deviceContext;
        public DX11TextureFactory(Device device, DeviceContext deviceContext)
        {
            this.device = device;
            this.deviceContext = deviceContext;
        }
        public ITexture2D CreateTexture2D(TextureDescription description)
        {
            DX11Texture2D texture = new DX11Texture2D(device, deviceContext, description);
            texture.Create();
            return texture;
        }

        public ITexture CreateTexture(TextureDescription description)
        {
            if (description.Type == TextureType.Texture2D)
            {
                return CreateTexture2D(description);
            }
            throw new NotImplementedException("RHI Backend does not implement switch case for other texture types: " + description.Type.ToString());
        }

        public ITexture3D CreateTexture3D()
        {
            throw new NotImplementedException();
        }

        public ITextureCube CreateTextureCube()
        {
            throw new NotImplementedException();
        }
    }
}
