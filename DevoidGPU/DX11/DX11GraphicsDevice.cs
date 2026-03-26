using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Numerics;

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
        private (int, int, int, int) viewportSize;

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
            viewportSize = (x, y, width, height);
        }

        public (int, int, int, int) GetViewport()
        {
            return viewportSize;
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

        private DX11Texture2D[] _boundPS_SRVs = new DX11Texture2D[16];
        private DX11Texture2D[] _boundVS_SRVs = new DX11Texture2D[16];
        private DX11Texture2D[] _boundCS_SRVs = new DX11Texture2D[16];
        private DX11Texture2D[] _boundCS_UAVs = new DX11Texture2D[8];
        private DX11Texture2D[] _boundRTVs = new DX11Texture2D[8];

        internal void TrackSRVBind(int slot, DX11Texture2D texture, ShaderStage stage)
        {
            if ((stage & ShaderStage.Fragment) != 0)
                _boundPS_SRVs[slot] = texture;

            if ((stage & ShaderStage.Vertex) != 0)
                _boundVS_SRVs[slot] = texture;

            if ((stage & ShaderStage.Compute) != 0)
                _boundCS_SRVs[slot] = texture;
        }

        //private void ResolveTextureHazard(DX11Texture2D texture)
        //{
        //    for (int i = 0; i < 16; i++)
        //    {
        //        if (_boundPS_SRVs[i] == texture)
        //        {
        //            deviceContext.PixelShader.SetShaderResource(i, null);
        //            _boundPS_SRVs[i] = null;
        //        }

        //        if (_boundVS_SRVs[i] == texture)
        //        {
        //            deviceContext.VertexShader.SetShaderResource(i, null);
        //            _boundVS_SRVs[i] = null;
        //        }

        //        if (_boundCS_SRVs[i] == texture)
        //        {
        //            deviceContext.ComputeShader.SetShaderResource(i, null);
        //            _boundCS_SRVs[i] = null;
        //        }
        //    }

        //    for (int i = 0; i < 8; i++)
        //    {
        //        if (_boundCS_UAVs[i] == texture)
        //        {
        //            deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
        //            _boundCS_UAVs[i] = null;
        //        }
        //    }
        //}

        void ResolveForSRV(DX11Texture2D tex)
        {
            for (int i = 0; i < 8; i++)
            {
                if (_boundCS_UAVs[i] == tex)
                {
                    deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    _boundCS_UAVs[i] = null;
                }
            }
        }

        void ResolveForUAV(DX11Texture2D tex)
        {
            for (int i = 0; i < 16; i++)
            {
                if (_boundPS_SRVs[i] == tex)
                {
                    deviceContext.PixelShader.SetShaderResource(i, null);
                    _boundPS_SRVs[i] = null;
                }

                if (_boundVS_SRVs[i] == tex)
                {
                    deviceContext.VertexShader.SetShaderResource(i, null);
                    _boundVS_SRVs[i] = null;
                }

                if (_boundCS_SRVs[i] == tex)
                {
                    deviceContext.ComputeShader.SetShaderResource(i, null);
                    _boundCS_SRVs[i] = null;
                }
            }
        }

        void ResolveForRTV(DX11Texture2D tex)
        {
            for (int i = 0; i < 16; i++)
            {
                if (_boundPS_SRVs[i] == tex)
                {
                    deviceContext.PixelShader.SetShaderResource(i, null);
                    _boundPS_SRVs[i] = null;
                }

                if (_boundVS_SRVs[i] == tex)
                {
                    deviceContext.VertexShader.SetShaderResource(i, null);
                    _boundVS_SRVs[i] = null;
                }

                if (_boundCS_SRVs[i] == tex)
                {
                    deviceContext.ComputeShader.SetShaderResource(i, null);
                    _boundCS_SRVs[i] = null;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (_boundCS_UAVs[i] == tex)
                {
                    deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    _boundCS_UAVs[i] = null;
                }
            }
        }


        public void BindTexture(ITexture texture, int slot, ShaderStage stage)
        {
            var tex = (DX11Texture2D)texture;

            ResolveForSRV(tex);

            if ((stage & ShaderStage.Vertex) != 0)
            {
                deviceContext.VertexShader.SetShaderResource(slot, tex.ShaderResourceView);
                _boundVS_SRVs[slot] = tex;
            }

            if ((stage & ShaderStage.Fragment) != 0)
            {
                deviceContext.PixelShader.SetShaderResource(slot, tex.ShaderResourceView);
                _boundPS_SRVs[slot] = tex;
            }

            if ((stage & ShaderStage.Compute) != 0)
            {
                deviceContext.ComputeShader.SetShaderResource(slot, tex.ShaderResourceView);
                _boundCS_SRVs[slot] = tex;
            }
        }
        public void BindTextureMutable(ITexture texture, int slot)
        {
            var tex = (DX11Texture2D)texture;

            ResolveForUAV(tex);

            deviceContext.ComputeShader.SetUnorderedAccessView(slot, tex.UnorderedAccessView);

            _boundCS_UAVs[slot] = tex;
        }

        public void BindFramebuffer(IFramebuffer fb)
        {
            Array.Clear(_boundRTVs);
            for (int i = 0; i < fb.ColorAttachments.Count; i++)
            {
                if (fb.ColorAttachments[i] is DX11Texture2D tex)
                {
                    ResolveForRTV(tex);
                    _boundRTVs[i] = tex;
                }
            }

            var rtvs = new RenderTargetView[fb.ColorAttachments.Count];
            for (int i = 0; i < fb.ColorAttachments.Count; i++)
            {
                if (fb.ColorAttachments[i] != null && fb.ColorAttachments[i] is DX11Texture2D tex2D)
                    rtvs[i] = tex2D.RenderTargetView;
            }

            DepthStencilView dsv = null;
            if (fb.DepthAttachment is DX11Texture2D depthTex)
                dsv = depthTex.DepthStencilView;

            deviceContext.OutputMerger.SetRenderTargets(dsv, rtvs);
        }

        public Matrix4x4 AdjustProjectionMatrix(Matrix4x4 projection)
        {
            // DX11 uses 0→1 depth range
            // No Y flip needed
            // No remapping needed
            return projection;
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

            Array.Clear(_boundPS_SRVs);
            Array.Clear(_boundVS_SRVs);
            Array.Clear(_boundCS_SRVs);
            Array.Clear(_boundCS_UAVs);
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