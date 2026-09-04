using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;
using SampleDescription = SharpDX.DXGI.SampleDescription;

namespace DevoidGPU.DX11
{
    internal sealed class DX11Texture : ITexture
    {
        public TextureDescription Description { get; }
        public int Width => Description.Width;
        public int Height => Description.Height;
        public TextureFormat Format => Description.Format;

        public ShaderResourceView? SRV { get; private set; }
        public RenderTargetView RTV => GetRTV();
        public DepthStencilView? DSV { get; private set; }
        public UnorderedAccessView? UAV { get; private set; }

        public Resource TextureResource { get; private set; } = null!;

        public TextureDimension Dimension => Description.Dimension;

        public int MipLevels => Description.MipLevels;

        public int ArraySize => Description.ArraySize;

        internal Format DX11Format;

        private readonly Device device;
        private readonly DeviceContext deviceContext = null!;
        private readonly bool ownsResource;
        private readonly RenderTargetView[,]? rtvs;

        internal DX11Texture(Device device, Texture2D existing)
        {
            this.device = device;

            TextureResource = existing;

            var desc = existing.Description;

            Description = new TextureDescription
            {
                Width = desc.Width,
                Height = desc.Height,
                Depth = 1,
                MipLevels = desc.MipLevels,
                ArraySize = desc.ArraySize,
                Format = DX11StateMapper.ToTextureFormat(desc.Format),

                Dimension = TextureDimension.Texture2D,

                Usage = TextureUsage.RenderTarget,
                Samples = new TextureSampleDescription(desc.SampleDescription.Count, desc.SampleDescription.Quality)
            };

            if (Description.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                rtvs = new RenderTargetView[Description.MipLevels, Description.ArraySize];
            }

            //RTV = CreateRTV2D(desc.Format);

            ownsResource = false;
            DX11Format = desc.Format;
        }

        public DX11Texture(Device device, DeviceContext deviceContext, TextureDescription description)
        {
            this.device = device;
            this.deviceContext = deviceContext;
            this.Description = description;

            Format format = DX11StateMapper.ResolveResourceFormat(Description);
            DX11Format = format;

            if (description.Dimension == TextureDimension.Texture3D)
                CreateTexture3D(format);
            else
                CreateTexture2D(format);

            if (Description.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                rtvs = new RenderTargetView[Description.MipLevels, Description.ArraySize];

                //if (Description.Dimension == TextureDimension.Texture3D)
                //    RTV = CreateRTV3D(format);
                //else
                //    RTV = CreateRTV2D(format);
            }

            if (Description.Usage.HasFlag(TextureUsage.DepthStencil))
                DSV = CreateDSV();

            if (Description.Usage.HasFlag(TextureUsage.ShaderResource))
                SRV = CreateSRV();

            if (Description.Usage.HasFlag(TextureUsage.UnorderedAccess))
            {
                if (Description.Samples.Count > 1)
                    throw new InvalidOperationException("MSAA textures cannot have UAVs.");

                UAV = new UnorderedAccessView(device, TextureResource);
            }

            ownsResource = true;
        }

        private void CreateTexture2D(Format format)
        {

            Texture2DDescription dxDescription = new()
            {
                Width = Description.Width,
                Height = Description.Height,
                Format = format,

                MipLevels = Description.MipLevels,
                ArraySize = Description.ArraySize,

                SampleDescription = new SampleDescription(Description.Samples.Count, Description.Samples.Quality),

                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = DX11StateMapper.ResolveTextureOptionFlags(Description),

                BindFlags = DX11StateMapper.ConvertBindFlags(Description.Usage)
            };

            TextureResource = new Texture2D(device, dxDescription);
        }

        private void CreateTexture3D(Format format)
        {

            Texture3DDescription dxDescription = new()
            {
                Width = Description.Width,
                Height = Description.Height,
                Depth = Description.Depth,

                MipLevels = Description.MipLevels,

                Format = format,

                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,

                BindFlags = DX11StateMapper.ConvertBindFlags(Description.Usage)
            };

            TextureResource = new Texture3D(device, dxDescription);
        }

        private ShaderResourceView CreateSRV()
        {
            Format srvFormat;
            var dimension = DX11StateMapper.ResolveSRVDimension(Description);

            if (Description.Usage.HasFlag(TextureUsage.DepthStencil))
            {
                if (dimension == ShaderResourceViewDimension.Texture3D)
                    throw new InvalidOperationException("3D textures cannot be depth stencil.");

                if (Description.Format == TextureFormat.Depth32_Float)
                    srvFormat = SharpDX.DXGI.Format.R32_Float;
                else
                    srvFormat = SharpDX.DXGI.Format.R24_UNorm_X8_Typeless;
            }
            else
            {
                srvFormat = DX11StateMapper.ToDXGITextureFormat(Description.Format);
            }

            var desc = new ShaderResourceViewDescription
            {
                Format = srvFormat,
                Dimension = dimension
            };

            switch (dimension)
            {
                case ShaderResourceViewDimension.Texture2D:
                    desc.Texture2D = new ShaderResourceViewDescription.Texture2DResource
                    {
                        MostDetailedMip = 0,
                        MipLevels = Description.MipLevels
                    };
                    break;

                case ShaderResourceViewDimension.Texture2DArray:
                    desc.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                    {
                        MostDetailedMip = 0,
                        MipLevels = Description.MipLevels,
                        FirstArraySlice = 0,
                        ArraySize = Description.ArraySize
                    };
                    break;

                case ShaderResourceViewDimension.TextureCube:
                    desc.TextureCube = new ShaderResourceViewDescription.TextureCubeResource
                    {
                        MostDetailedMip = 0,
                        MipLevels = Description.MipLevels
                    };
                    break;

                case ShaderResourceViewDimension.Texture3D:
                    desc.Texture3D = new ShaderResourceViewDescription.Texture3DResource
                    {
                        MostDetailedMip = 0,
                        MipLevels = Description.MipLevels
                    };
                    break;
            }

            return new ShaderResourceView(device, TextureResource, desc);
        }

        private DepthStencilView CreateDSV()
        {
            if (Description.Dimension == TextureDimension.Texture3D)
                throw new InvalidOperationException("3D textures cannot be depth-stencil targets.");

            Format dsvFormat;

            if (Description.Format == TextureFormat.Depth32_Float)
                dsvFormat = SharpDX.DXGI.Format.D32_Float;
            else
                dsvFormat = SharpDX.DXGI.Format.D24_UNorm_S8_UInt;

            if (Description.Samples.Count > 1)
            {
                if (Description.ArraySize > 1)
                {
                    var desc = new DepthStencilViewDescription
                    {
                        Format = dsvFormat,
                        Dimension = DepthStencilViewDimension.Texture2DMultisampledArray,

                        Texture2DMSArray =
                            new DepthStencilViewDescription.Texture2DMultisampledArrayResource
                            {
                                FirstArraySlice = 0,
                                ArraySize = Description.ArraySize
                            }
                    };

                    return new DepthStencilView(device, TextureResource, desc);
                }

                // Normal 2D MSAA texture
                var msaaDesc = new DepthStencilViewDescription
                {
                    Format = dsvFormat,
                    Dimension = DepthStencilViewDimension.Texture2DMultisampled
                };

                return new DepthStencilView(device, TextureResource, msaaDesc);
            }

            var normalDesc = new DepthStencilViewDescription
            {
                Format = dsvFormat,
                Dimension = DepthStencilViewDimension.Texture2D,

                Texture2D = new DepthStencilViewDescription.Texture2DResource
                {
                    MipSlice = 0
                }
            };

            return new DepthStencilView(device, TextureResource, normalDesc);
        }

        internal RenderTargetView GetRTV(int mip = 0, int slice = 0)
        {
            if (rtvs == null)
                throw new InvalidOperationException(
                    "Texture was not created with RenderTarget usage.");

            var rtv = rtvs[mip, slice];

            if (rtv == null)
            {
                rtv = CreateRTV(
                    DX11StateMapper.ToDXGITextureFormat(Description.Format),
                    mip,
                    slice);

                rtvs[mip, slice] = rtv;
            }

            return rtv;
        }

        private RenderTargetView CreateRTV2D(Format format)
        {
            if (Description.ArraySize > 1)
            {
                var desc = new RenderTargetViewDescription
                {
                    Format = format,
                    Dimension = RenderTargetViewDimension.Texture2DArray,

                    Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource
                    {
                        FirstArraySlice = 0,
                        ArraySize = Description.ArraySize,
                        MipSlice = 0
                    }
                };

                return new RenderTargetView(device, TextureResource, desc);
            }

            return new RenderTargetView(device, TextureResource);
        }

        private RenderTargetView CreateRTV3D(Format format)
        {
            var desc = new RenderTargetViewDescription
            {
                Format = format,
                Dimension = RenderTargetViewDimension.Texture3D,

                Texture3D = new RenderTargetViewDescription.Texture3DResource
                {
                    FirstDepthSlice = 0,
                    DepthSliceCount = 1,
                    MipSlice = 0
                }
            };

            return new RenderTargetView(device, TextureResource, desc);
        }

        private RenderTargetView CreateRTV(Format format, int mip, int slice)
        {
            if (Description.Dimension == TextureDimension.Texture3D)
            {
                return new RenderTargetView(device, TextureResource,
                    new RenderTargetViewDescription
                    {
                        Format = format,
                        Dimension = RenderTargetViewDimension.Texture3D,
                        Texture3D =
                            new RenderTargetViewDescription.Texture3DResource
                            {
                                FirstDepthSlice = slice,
                                DepthSliceCount = 1,
                                MipSlice = mip
                            }
                    }
                );
            }

            if (Description.Samples.Count > 1)
            {
                if (Description.ArraySize > 1)
                {
                    return new RenderTargetView(device, TextureResource,
                        new RenderTargetViewDescription
                        {
                            Format = format,
                            Dimension = RenderTargetViewDimension.Texture2DMultisampledArray,

                            Texture2DMSArray =
                                new RenderTargetViewDescription
                                    .Texture2DMultisampledArrayResource
                                {
                                    FirstArraySlice = slice,
                                    ArraySize = 1
                                }
                        }
                    );
                }

                return new RenderTargetView(device, TextureResource,
                    new RenderTargetViewDescription
                    {
                        Format = format,
                        Dimension = RenderTargetViewDimension.Texture2DMultisampled
                    }
                );
            }

            if (Description.Dimension == TextureDimension.TextureCube ||
                    Description.ArraySize > 1)
            {
                return new RenderTargetView(device, TextureResource,
                    new RenderTargetViewDescription
                    {
                        Format = format,
                        Dimension = RenderTargetViewDimension.Texture2DArray,

                        Texture2DArray =
                            new RenderTargetViewDescription.Texture2DArrayResource
                            {
                                FirstArraySlice = slice,
                                ArraySize = 1,
                                MipSlice = mip
                            }
                    }
                );
            }

            // Normal 2D
            return new RenderTargetView(device, TextureResource,
                new RenderTargetViewDescription
                {
                    Format = format,
                    Dimension = RenderTargetViewDimension.Texture2D,

                    Texture2D =
                        new RenderTargetViewDescription.Texture2DResource
                        {
                            MipSlice = mip
                        }
                }
            );
        }
        public void Update(ReadOnlySpan<byte> data)
        {
            unsafe
            {
                int rowPitch = GetRowPitch();
                int slicePitch = GetSlicePitch();

                int expectedSize =
                    slicePitch *
                    Description.ArraySize;

                if (data.Length < expectedSize)
                {
                    throw new ArgumentException(
                        $"Insufficient texture data. " +
                        $"Expected at least {expectedSize} bytes, " +
                        $"got {data.Length} bytes.");
                }

                fixed (byte* ptr = data)
                {
                    for (int slice = 0;
                         slice < Description.ArraySize;
                         slice++)
                    {
                        byte* slicePtr =
                            ptr + slice * slicePitch;

                        DataBox box = new(
                            (IntPtr)slicePtr,
                            rowPitch,
                            slicePitch);

                        int subresource =
                            slice * Description.MipLevels;

                        deviceContext.UpdateSubresource(
                            box,
                            TextureResource,
                            subresource);
                    }
                }
            }
        }

        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            Update(MemoryMarshal.AsBytes(data));
        }


        private int GetRowPitch()
        {
            return Description.Width * DX11StateMapper.BytesPerPixel(Format);
        }

        private int GetSlicePitch()
        {
            return GetRowPitch() * Description.Height;
        }
        public void Dispose()
        {
            UAV?.Dispose();
            SRV?.Dispose();
            if (rtvs != null)
            {
                foreach (var rtv in rtvs)
                    rtv?.Dispose();
            }
            DSV?.Dispose();

            if (ownsResource)
                TextureResource.Dispose();
        }
    }
}
