using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;

namespace DevoidGPU.DX11
{
    internal class DX11TextureCube : IDX11Texture, ITextureCube
    {
        private IntPtr handle;

        public TextureType Type => TextureType.Texture2D;
        public int Width { get; set; }

        public int Height { get; set; }

        public bool IsRenderTarget { get; }

        public bool IsDepthStencil { get; }

        public Texture2D Texture { get; set; }
        public RenderTargetView RenderTargetView { get; set; }
        public DepthStencilView DepthStencilView { get; set; }
        public ShaderResourceView ShaderResourceView { get; private set; }
        public UnorderedAccessView UnorderedAccessView { get; private set; }

        public bool AllowUnorderedView { get; set; }

        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private TextureDescription Description;
        private Format format;
        private RenderTargetView[,] rtvCache;

        public DX11TextureCube(Device device, DeviceContext context, TextureDescription desc)
        {
            this.device = device;
            this.deviceContext = context;
            this.Description = desc;

            this.format = DX11TextureFormat.ToDXGIFormat(desc.Format);

            if (desc.Width != desc.Height)
                throw new Exception("Cubemap must be square");

            this.Width = Description.Width;
            this.Height = Description.Height;
            this.IsRenderTarget = desc.IsRenderTarget;
        }

        public void Create()
        {
            var texDesc = new Texture2DDescription
            {
                Width = Width,
                Height = Height,
                MipLevels = Description.MipLevels,
                ArraySize = 6,
                Format = format,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.TextureCube |
                              (Description.GenerateMipmaps ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None)
            };

            Texture = new Texture2D(device, texDesc);

            if (IsRenderTarget)
            {
                int mipCount = Description.MipLevels > 0 ? Description.MipLevels : 1;
                rtvCache = new RenderTargetView[6, mipCount];
            }

            //var srvDesc = new ShaderResourceViewDescription
            //{
            //    Format = format,
            //    Dimension = ShaderResourceViewDimension.TextureCube,
            //    TextureCube = new ShaderResourceViewDescription.TextureCubeResource
            //    {
            //        MostDetailedMip = 0,
            //        MipLevels = texDesc.MipLevels,
            //    }
            //};

            ShaderResourceView = new ShaderResourceView(device, Texture);

            handle = TextureManager.Register(this);
        }

        public void SetMainFace(CubeFace face, int mipLevel = 0)
        {
            this.RenderTargetView = GetRTV((int)face, mipLevel);
        }

        public RenderTargetView GetRTV(int face, int mip = 0)
        {
            if (!IsRenderTarget)
                throw new Exception("Texture is not render-target capable");

            if (rtvCache[face, mip] != null)
                return rtvCache[face, mip];

            var desc = new RenderTargetViewDescription
            {
                Format = format,
                Dimension = RenderTargetViewDimension.Texture2DArray,
                Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource
                {
                    MipSlice = mip,
                    FirstArraySlice = face,
                    ArraySize = 1
                }
            };

            var rtv = new RenderTargetView(device, Texture, desc);
            rtvCache[face, mip] = rtv;

            return rtv;
        }

        public void SetData(CubeFace face, byte[] data)
        {
            int faceIndex = (int)face;
            if (faceIndex < 0 || faceIndex >= 6)
                throw new Exception("Invalid cubemap face");

            int subresource = SharpDX.Direct3D11.Resource.CalculateSubResourceIndex(0, faceIndex, Description.MipLevels);

            int rowPitch = DX11TextureFormat.RowPitch(
                DX11TextureFormat.DXGIFormatToTextureFormat(format),
                Width
            );

            var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
            var box = new DataBox(ptr, rowPitch, 0);

            deviceContext.UpdateSubresource(box, Texture, subresource);
        }

        public void GenerateMipmaps()
        {
            deviceContext.GenerateMips(ShaderResourceView);
        }

        public IntPtr GetHandle() => handle;

        public void Dispose()
        {
            TextureManager.Unregister(handle);

            if (rtvCache != null)
            {
                for (int face = 0; face < rtvCache.GetLength(0); face++)
                {
                    for (int mip = 0; mip < rtvCache.GetLength(1); mip++)
                    {
                        rtvCache[face, mip]?.Dispose();
                        rtvCache[face, mip] = null;
                    }
                }
            }

            ShaderResourceView?.Dispose();
            Texture?.Dispose();

            ShaderResourceView = null;
            Texture = null;

            handle = IntPtr.Zero; 
            UnorderedAccessView?.Dispose();
            DepthStencilView?.Dispose();
        }
    }
}