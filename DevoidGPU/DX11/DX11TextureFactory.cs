﻿using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ITexture2D CreateTexture2D(Tex2DDescription description)
        {
            DX11Texture2D texture = new DX11Texture2D(device, deviceContext, description);
            texture.Create();
            return texture;
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
