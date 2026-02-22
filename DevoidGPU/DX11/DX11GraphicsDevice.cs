using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DevoidGPU.DX11
{
    public class DX11GraphicsDevice : IGraphicsDevice
    {

        public IPresentSurface MainSurface { get; private set; }

        public IBufferFactory BufferFactory { get; private set; }
        public IShaderFactory ShaderFactory { get; private set; }
        public ITextureFactory TextureFactory { get; private set; }

        private SharpDX.Direct3D11.Device device;
        private DeviceContext deviceContext;
        private SwapChain swapChain;

        public void Initialize(nint hwnd, PresentationParameters parameters)
        {
            var swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 2,
                ModeDescription = new ModeDescription()
                {
                    Width = parameters.BackBufferWidth,
                    Height = parameters.BackBufferHeight,
                    RefreshRate = new Rational(parameters.RefreshRate, 1),
                    Format = DX11TextureFormat.ToDXGIFormat(parameters.ColorFormat)
                },
                IsWindowed = parameters.Windowed,
                Usage = Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.Discard,

                SampleDescription = new SampleDescription(1, 0),

                OutputHandle = hwnd
            };

            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.BgraSupport
                #if DEBUG 
                | DeviceCreationFlags.Debug 
                #endif
                ,
                swapChainDesc,
                out device,
                out swapChain
            );


            deviceContext = device.ImmediateContext;

            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
            {
                var rtv = new RenderTargetView(device, backBuffer);
                MainSurface = new DX11PresentSurface(device, deviceContext, swapChain, rtv);
            }

            MainSurface.VSync = parameters.VSync;

            BufferFactory = new DX11BufferFactory(device, deviceContext);
            ShaderFactory = new DX11ShaderFactory(device, deviceContext);
            TextureFactory = new DX11TextureFactory(device, deviceContext);
        }

        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }

        public void Draw(int vertexCount, int startVertex)
        {
            deviceContext.Draw(vertexCount, startVertex);
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            deviceContext.Rasterizer.SetViewport(x, y, width, height);
        }

        public IInputLayout CreateInputLayout(VertexInfo vertexInfo, IShader vertexShader)
        {
            if (vertexShader is not DX11Shader dxShader)
                throw new ArgumentException("Vertex shader must be DX11Shader", nameof(vertexShader));

            return new DX11InputLayout(device, deviceContext, vertexInfo, dxShader);
        }

        public ISampler CreateSampler(SamplerDescription description)
        {
            DX11Sampler sampler = new DX11Sampler(device, deviceContext, description);
            return sampler;
        }

        public ITexture GetTexture(nint handle)
        {
            return TextureManager.Resolve(handle);
        }






        private readonly Dictionary<(BlendMode, bool), BlendState> blendCache = new();
        private readonly Dictionary<(DepthTest, bool), DepthStencilState> depthCache = new();
        private readonly Dictionary<(CullMode, FillMode), RasterizerState> rasterizerCache = new();
        private readonly Dictionary<(int x, int y, int w, int h), SharpDX.Mathematics.Interop.RawRectangle> scissorCache = new();

        private BlendState currentBlend;
        private DepthStencilState currentDepthStencil;
        private RasterizerState currentRasterizer;
        private SharpDX.Mathematics.Interop.RawRectangle? currentScissor;

        public void UnbindAllShaderResources()
        {
            ShaderResourceView[] nullSRVs = new ShaderResourceView[16];
            UnorderedAccessView[] nullUAVs = new UnorderedAccessView[7];

            deviceContext.PixelShader.SetShaderResources(0, 16, nullSRVs);
            deviceContext.VertexShader.SetShaderResources(0, 16, nullSRVs);

            deviceContext.ComputeShader.SetUnorderedAccessViews(0, nullUAVs);
        }


        public void SetRasterizerState(CullMode cullMode, FillMode fillMode = FillMode.Solid)
        {
            var key = (cullMode, fillMode);

            if (!rasterizerCache.TryGetValue(key, out var state))
            {
                var desc = new RasterizerStateDescription
                {
                    CullMode = DX11StateMapper.ToDXCullMode(cullMode),
                    FillMode = DX11StateMapper.ToDXFillMode(fillMode),
                    IsFrontCounterClockwise = false,
                    IsDepthClipEnabled = true
                };

                state = new RasterizerState(device, desc);
                rasterizerCache[key] = state;
            }

            if (currentRasterizer == state) return;
            deviceContext.Rasterizer.State = state;
            currentRasterizer = state;
        }


        public void SetBlendState(BlendMode mode, bool alphaToCoverage = false)
        {
            var key = (mode, alphaToCoverage);

            if (!blendCache.TryGetValue(key, out var state))
            {
                var desc = new BlendStateDescription
                {
                    AlphaToCoverageEnable = alphaToCoverage,
                    IndependentBlendEnable = false
                };

                var rt = new RenderTargetBlendDescription
                {
                    IsBlendEnabled = (mode != BlendMode.Opaque),
                    RenderTargetWriteMask = ColorWriteMaskFlags.All
                };

                switch (mode)
                {
                    case BlendMode.Opaque:
                        rt.SourceBlend = BlendOption.One;
                        rt.DestinationBlend = BlendOption.Zero;
                        rt.BlendOperation = BlendOperation.Add;

                        rt.SourceAlphaBlend = BlendOption.One;
                        rt.DestinationAlphaBlend = BlendOption.Zero;
                        rt.AlphaBlendOperation = BlendOperation.Add;
                        break;

                    case BlendMode.AlphaBlend:
                        rt.SourceBlend = BlendOption.SourceAlpha;
                        rt.DestinationBlend = BlendOption.InverseSourceAlpha;
                        rt.BlendOperation = BlendOperation.Add;

                        rt.SourceAlphaBlend = BlendOption.One;
                        rt.DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
                        rt.AlphaBlendOperation = BlendOperation.Add;
                        break;

                    case BlendMode.Additive:
                        rt.SourceBlend = BlendOption.One;
                        rt.DestinationBlend = BlendOption.One;
                        rt.BlendOperation = BlendOperation.Add;

                        rt.SourceAlphaBlend = BlendOption.One;
                        rt.DestinationAlphaBlend = BlendOption.One;
                        rt.AlphaBlendOperation = BlendOperation.Add;
                        break;
                }

                desc.RenderTarget[0] = rt;
                state = new BlendState(device, desc);
                blendCache[key] = state;
            }

            if (currentBlend == state) return; // already bound
            deviceContext.OutputMerger.SetBlendState(state);
            currentBlend = state;
        }


        public void SetDepthState(DepthTest func, bool writeEnable)
        {
            var key = (func, writeEnable);

            if (!depthCache.TryGetValue(key, out var state))
            {
                var desc = new DepthStencilStateDescription
                {
                    IsDepthEnabled = func != DepthTest.Disabled, // only enable if not Disabled
                    DepthComparison = DX11StateMapper.ToDXDepthFunc(func),
                    DepthWriteMask = writeEnable ? DepthWriteMask.All : DepthWriteMask.Zero
                };


                state = new DepthStencilState(device, desc);
                depthCache[key] = state;
            }

            if (currentDepthStencil == state) return;
            deviceContext.OutputMerger.SetDepthStencilState(state);
            currentDepthStencil = state;
        }

        // In DX11GraphicsDevice
        private readonly Dictionary<(CullMode, FillMode, bool), RasterizerState> rasterizerCache2 = new();
        private CullMode currentCull = CullMode.Back;
        private FillMode currentFill = FillMode.Solid;
        private bool currentScissorEnabled = false;

        public void SetRasterizerState(CullMode cullMode, FillMode fillMode = FillMode.Solid, bool scissorEnabled = false)
        {
            currentCull = cullMode;
            currentFill = fillMode;
            currentScissorEnabled = scissorEnabled;

            var key = (cullMode, fillMode, scissorEnabled);
            if (!rasterizerCache2.TryGetValue(key, out var state))
            {
                var desc = new RasterizerStateDescription
                {
                    CullMode = DX11StateMapper.ToDXCullMode(cullMode),
                    FillMode = DX11StateMapper.ToDXFillMode(fillMode),
                    IsFrontCounterClockwise = false,
                    IsDepthClipEnabled = true,
                    IsScissorEnabled = scissorEnabled
                };
                state = new RasterizerState(device, desc);
                rasterizerCache2[key] = state;
            }
            deviceContext.Rasterizer.State = state;
        }

        // Enable/disable scissor without obliterating cull/fill
        public void SetScissorState(bool enable)
        {
            if (enable == currentScissorEnabled) return;
            SetRasterizerState(currentCull, currentFill, enable);
        }

        // Set only the rectangle
        public void SetScissorRectangle(int x, int y, int width, int height)
        {
            deviceContext.Rasterizer.SetScissorRectangle(x, y, x + width, y + height);
        }


        public void SetStencilState(
            bool enable,
            CompareFunc func,
            int reference,
            StencilOp stencilFail = StencilOp.Keep,
            StencilOp depthFail = StencilOp.Keep,
            StencilOp pass = StencilOp.Keep)
        {
            var desc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.LessEqual,

                IsStencilEnabled = enable,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = DX11StateMapper.ToDXStencilOp(stencilFail),
                    DepthFailOperation = DX11StateMapper.ToDXStencilOp(depthFail),
                    PassOperation = DX11StateMapper.ToDXStencilOp(pass),
                    Comparison = DX11StateMapper.ToDXComparison(func)
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = DX11StateMapper.ToDXStencilOp(stencilFail),
                    DepthFailOperation = DX11StateMapper.ToDXStencilOp(depthFail),
                    PassOperation = DX11StateMapper.ToDXStencilOp(pass),
                    Comparison = DX11StateMapper.ToDXComparison(func)
                }
            };

            var newState = new DepthStencilState(device, desc);

            // Avoid rebinding the same state
            if (currentDepthStencil != newState)
            {
                deviceContext.OutputMerger.SetDepthStencilState(newState, reference);
                currentDepthStencil = newState;
            }
        }

        public void SetPrimitiveType(PrimitiveType primitiveType)
        {
            deviceContext.InputAssembler.PrimitiveTopology = DX11StateMapper.ToDXPrimitiveType(primitiveType);
        }
    }
}
