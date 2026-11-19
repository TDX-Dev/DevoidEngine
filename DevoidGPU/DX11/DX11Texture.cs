using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;
using SampleDescription = SharpDX.DXGI.SampleDescription;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11Texture : ITexture
    {
        public TextureDescription Description { get; }
        public int Width => Description.Width;
        public int Height => Description.Height;
        public TextureFormat Format => Description.Format;

        public ShaderResourceView? Srv { get; private set; }
        public RenderTargetView Rtv => GetRtv();
        public DepthStencilView? Dsv { get; private set; }
        public UnorderedAccessView? Uav { get; private set; }

        public Resource TextureResource { get; private set; } = null!;

        public TextureDimension Dimension => Description.Dimension;

        public int MipLevels => Description.MipLevels;

        public int ArraySize => Description.ArraySize;

        internal Format Dx11Format;

        private readonly Device _device;
        private readonly DeviceContext _deviceContext = null!;
        private readonly bool _ownsResource;
        private readonly RenderTargetView[,]? _rtvs;

        internal Dx11Texture(Device device, Texture2D existing)
        {
            this._device = device;

            TextureResource = existing;

            var desc = existing.Description;

            Description = new TextureDescription
            {
                Width = desc.Width,
                Height = desc.Height,
                Depth = 1,
                MipLevels = desc.MipLevels,
                ArraySize = desc.ArraySize,
                Format = Dx11StateMapper.ToTextureFormat(desc.Format),

                Dimension = TextureDimension.Texture2D,

                Usage = TextureUsage.RenderTarget,
                Samples = new TextureSampleDescription(desc.SampleDescription.Count, desc.SampleDescription.Quality)
            };

            if (Description.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                _rtvs = new RenderTargetView[Description.MipLevels, Description.ArraySize];
            }

            //RTV = CreateRTV2D(desc.Format);

            _ownsResource = false;
            Dx11Format = desc.Format;
        }

        public Dx11Texture(Device device, DeviceContext deviceContext, TextureDescription description)
        {
            this._device = device;
            this._deviceContext = deviceContext;
            this.Description = description;

            Format format = Dx11StateMapper.ResolveResourceFormat(Description);
            Dx11Format = format;

            if (description.Dimension == TextureDimension.Texture3D)
                CreateTexture3D(format);
            else
                CreateTexture2D(format);

            if (Description.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                _rtvs = new RenderTargetView[Description.MipLevels, Description.ArraySize];

                //if (Description.Dimension == TextureDimension.Texture3D)
                //    RTV = CreateRTV3D(format);
                //else
                //    RTV = CreateRTV2D(format);
            }

            if (Description.Usage.HasFlag(TextureUsage.DepthStencil))
                Dsv = CreateDsv();

            if (Description.Usage.HasFlag(TextureUsage.ShaderResource))
                Srv = CreateSrv();

            if (Description.Usage.HasFlag(TextureUsage.UnorderedAccess))
            {
                if (Description.Samples.Count > 1)
                    throw new InvalidOperationException("MSAA textures cannot have UAVs.");

                Uav = new UnorderedAccessView(device, TextureResource);
            }

            _ownsResource = true;
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

                Usage = Dx11StateMapper.ToDxResourceUsage(Description.ResourceUsage),
                CpuAccessFlags = Dx11StateMapper.ToDxCpuAccess(Description.CpuAccess),
                OptionFlags = Dx11StateMapper.ResolveTextureOptionFlags(Description),

                BindFlags = Dx11StateMapper.ConvertBindFlags(Description.Usage)
            };

            TextureResource = new Texture2D(_device, dxDescription);
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

                BindFlags = Dx11StateMapper.ConvertBindFlags(Description.Usage)
            };

            TextureResource = new Texture3D(_device, dxDescription);
        }

        private ShaderResourceView CreateSrv()
        {
            Format srvFormat;
            var dimension = Dx11StateMapper.ResolveSrvDimension(Description);

            if (Description.Usage.HasFlag(TextureUsage.DepthStencil))
            {
                if (dimension == ShaderResourceViewDimension.Texture3D)
                    throw new InvalidOperationException("3D textures cannot be depth stencil.");

                if (Description.Format == TextureFormat.Depth32Float)
                    srvFormat = SharpDX.DXGI.Format.R32_Float;
                else
                    srvFormat = SharpDX.DXGI.Format.R24_UNorm_X8_Typeless;
            }
            else
            {
                srvFormat = Dx11StateMapper.ToDxgiTextureFormat(Description.Format);
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

            return new ShaderResourceView(_device, TextureResource, desc);
        }

        private DepthStencilView CreateDsv()
        {
            if (Description.Dimension == TextureDimension.Texture3D)
                throw new InvalidOperationException("3D textures cannot be depth-stencil targets.");

            Format dsvFormat;

            if (Description.Format == TextureFormat.Depth32Float)
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

                    return new DepthStencilView(_device, TextureResource, desc);
                }

                // Normal 2D MSAA texture
                var msaaDesc = new DepthStencilViewDescription
                {
                    Format = dsvFormat,
                    Dimension = DepthStencilViewDimension.Texture2DMultisampled
                };

                return new DepthStencilView(_device, TextureResource, msaaDesc);
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

            return new DepthStencilView(_device, TextureResource, normalDesc);
        }

        internal RenderTargetView GetRtv(int mip = 0, int slice = 0)
        {
            if (_rtvs == null)
                throw new InvalidOperationException(
                    "Texture was not created with RenderTarget usage.");

            var rtv = _rtvs[mip, slice];

            if (rtv == null)
            {
                rtv = CreateRtv(
                    Dx11StateMapper.ToDxgiTextureFormat(Description.Format),
                    mip,
                    slice);

                _rtvs[mip, slice] = rtv;
            }

            return rtv;
        }

        private RenderTargetView CreateRtv2D(Format format)
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

                return new RenderTargetView(_device, TextureResource, desc);
            }

            return new RenderTargetView(_device, TextureResource);
        }

        private RenderTargetView CreateRtv3D(Format format)
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

            return new RenderTargetView(_device, TextureResource, desc);
        }

        private RenderTargetView CreateRtv(Format format, int mip, int slice)
        {
            if (Description.Dimension == TextureDimension.Texture3D)
            {
                return new RenderTargetView(_device, TextureResource,
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
                    return new RenderTargetView(_device, TextureResource,
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

                return new RenderTargetView(_device, TextureResource,
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
                return new RenderTargetView(_device, TextureResource,
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
            return new RenderTargetView(_device, TextureResource,
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

                        _deviceContext.UpdateSubresource(
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
            return Description.Width * Dx11StateMapper.BytesPerPixel(Format);
        }

        private int GetSlicePitch()
        {
            return GetRowPitch() * Description.Height;
        }
        public void Dispose()
        {
            Uav?.Dispose();
            Srv?.Dispose();
            if (_rtvs != null)
            {
                foreach (var rtv in _rtvs)
                    rtv?.Dispose();
            }
            Dsv?.Dispose();

            if (_ownsResource)
                TextureResource.Dispose();
        }
    }
}
