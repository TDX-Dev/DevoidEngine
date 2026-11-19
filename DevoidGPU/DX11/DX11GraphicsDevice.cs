using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using DeviceCreationFlags = SharpDX.Direct3D11.DeviceCreationFlags;
using DriverType = SharpDX.Direct3D.DriverType;
using DXGIDevice = SharpDX.DXGI.Device;

namespace DevoidGPU.DX11
{
    public sealed class Dx11GraphicsDevice : IGraphicsDevice
    {
        private readonly Factory1 _factory = null!;
        private readonly Device _device = null!;
        private readonly DeviceContext _deviceContext = null!;

        private readonly ICommandQueue _graphicsQueue;
        private readonly ICommandQueue _computeQueue;
        private readonly ICommandQueue _copyQueue;

        private readonly Dx11CommandList _cachedCommandList;
        private readonly Adapter3 _adapter;

        public GraphicsDeviceInfo Info { get; private set; }

        public Dx11GraphicsDevice()
        {
            _factory = new Factory1();

            var levels = new[]
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0
            };


            _device = new Device(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport
#if DEBUG
                | DeviceCreationFlags.Debug
#endif
                , levels
            );


            using var dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device>();
            _adapter = dxgiDevice.Adapter.QueryInterface<Adapter3>();

            AdapterDescription adapterDescription = _adapter.Description;

            Info = new GraphicsDeviceInfo
            {
                Name = adapterDescription.Description,

                DedicatedVideoMemory = (ulong)(long)adapterDescription.DedicatedVideoMemory,
                DedicatedSystemMemory = (ulong)(long)adapterDescription.DedicatedSystemMemory,
                SharedSystemMemory = (ulong)(long)adapterDescription.SharedSystemMemory,

                VendorId = adapterDescription.VendorId,
                DeviceId = adapterDescription.DeviceId,
                FeatureLevel = (int)_device.FeatureLevel
            };


            _deviceContext = _device.ImmediateContext;

            // Queue Creation

            var queue = new Dx11CommandQueue(_deviceContext);

            _graphicsQueue = queue;
            _computeQueue = queue;
            _copyQueue = queue;

            _cachedCommandList = new Dx11CommandList(_deviceContext);
        }

        public ISwapchain CreateSwapchain(SwapchainDescription desc)
        {
            return new Dx11SwapChain(_factory, _device, desc);
        }

        public IShader CreateShader(ShaderDescription desc)
        {
            return new Dx11Shader(_device, desc);
        }

        public IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription desc)
        {
            Dx11GraphicsPipeline dxPipeline = new()
            {
                Vs = ((Dx11Shader)desc.VertexShader).Vs!,
                Ps = ((Dx11Shader)desc.PixelShader).Ps!,

                Topology = Dx11StateMapper.ToDxPrimitiveType(desc.Topology)
            };


            // Blend state
            BlendState[] blendStates = desc.Blend.BlendStates;
            int count = Math.Min(blendStates.Length, 8);

            SharpDX.Direct3D11.BlendStateDescription blendStateDesc = new()
            {
                AlphaToCoverageEnable = desc.Blend.AlphaToCoverage,
                IndependentBlendEnable = desc.Blend.IndependentBlend
            };

            for (int i = 0; i < count; i++)
            {
                BlendState blendState = blendStates[i];
                blendStateDesc.RenderTarget[i] = new()
                {
                    IsBlendEnabled = blendState.Enable,

                    SourceBlend = Dx11StateMapper.ToDxBlend(blendState.SrcColor),
                    DestinationBlend = Dx11StateMapper.ToDxBlend(blendState.DstColor),
                    BlendOperation = Dx11StateMapper.ToDxBlendOp(blendState.ColorOp),

                    SourceAlphaBlend = Dx11StateMapper.ToDxBlend(blendState.SrcAlpha),
                    DestinationAlphaBlend = Dx11StateMapper.ToDxBlend(blendState.DstAlpha),
                    AlphaBlendOperation = Dx11StateMapper.ToDxBlendOp(blendState.AlphaOp),

                    RenderTargetWriteMask = Dx11StateMapper.ToDxColorMask(blendState.WriteMask),
                };
            }

            dxPipeline.BlendState = new SharpDX.Direct3D11.BlendState(_device, blendStateDesc);

            // Rasterizer State

            SharpDX.Direct3D11.RasterizerStateDescription rasterizerStateDesc = new()
            {
                CullMode = Dx11StateMapper.ToDxCullMode(desc.Rasterizer.CullMode),
                FillMode = Dx11StateMapper.ToDxFillMode(desc.Rasterizer.FillMode),
                IsFrontCounterClockwise = desc.Rasterizer.FrontCounterClockwise,

                IsScissorEnabled = desc.Rasterizer.EnableScissor
            };

            dxPipeline.RasterizerState = new SharpDX.Direct3D11.RasterizerState(_device, rasterizerStateDesc);

            // Depth State

            SharpDX.Direct3D11.DepthStencilStateDescription depthStateDesc = new()
            {
                IsDepthEnabled = desc.DepthStencil.DepthTest,
                DepthComparison = Dx11StateMapper.ToDxDepthComparison(desc.DepthStencil.DepthFunc),
                DepthWriteMask = desc.DepthStencil.DepthWrite ? SharpDX.Direct3D11.DepthWriteMask.All : SharpDX.Direct3D11.DepthWriteMask.Zero
            };

            dxPipeline.DepthStencilState = new SharpDX.Direct3D11.DepthStencilState(_device, depthStateDesc);

            // InputLayout

            var dxVs = (Dx11Shader)desc.VertexShader;

            dxPipeline.InputLayout = new SharpDX.Direct3D11.InputLayout(
                _device,
                dxVs.Bytecode,
                Dx11StateMapper.CreateInputElements(desc.VertexLayout)
            );

            dxPipeline.Layout = (Dx11PipelineLayout)desc.PipelineLayout;

            return dxPipeline;
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDescription desc)
        {
            return new Dx11ComputePipeline(desc);
        }

        public IVertexBuffer CreateVertexBuffer(VertexBufferDescription desc)
        {
            return new Dx11VertexBuffer(_device, _deviceContext, desc);
        }
        public IIndexBuffer CreateIndexBuffer(IndexBufferDescription desc)
        {
            return new Dx11IndexBuffer(_device, _deviceContext, desc);
        }
        public ITexture CreateTexture(TextureDescription desc)
        {
            return new Dx11Texture(_device, _deviceContext, desc);
        }

        public ISampler CreateSampler(SamplerDescription desc)
        {
            return new Dx11Sampler(_device, desc);
        }
        public IFrameBuffer CreateFrameBuffer(ITexture?[] colorAttachments, ITexture? depthAttachment)
        {
            return new Dx11Framebuffer([.. colorAttachments.Cast<Dx11Texture>()], depthAttachment as Dx11Texture);
        }
        public IUniformBuffer CreateUniformBuffer(BufferDescription desc)
        {
            return new Dx11UniformBuffer(_device, _deviceContext, desc);
        }

        public IShaderStorageBuffer CreateShaderStorageBuffer(BufferDescription desc)
        {
            return new Dx11ShaderStorageBuffer(_device, _deviceContext, desc);
        }

        public IDescriptorLayout CreateDescriptorLayout(DescriptorBinding[] bindings)
        {
            return new Dx11DescriptorLayout(bindings);
        }

        public IDescriptorSet CreateDescriptorSet(IDescriptorLayout layout)
        {
            return new Dx11DescriptorSet(layout);
        }
        public IGpuTimestamp CreateGpuTimestamp()
        {
            return new Dx11GpuTimestamp(_device, _device.ImmediateContext);
        }
        public IGpuTimestampDisjoint CreateGpuTimestampDisjoint()
        {
            return new Dx11GpuTimestampDisjoint(_device, _device.ImmediateContext);
        }
        public ICommandList GetCommandList()
        {
            _cachedCommandList.Reset();
            return _cachedCommandList;
        }
        public ICommandQueue GetCommandQueue(CommandListType type)
        {
            return type switch
            {
                CommandListType.Graphics => _graphicsQueue,
                CommandListType.Compute => _computeQueue,
                CommandListType.Copy => _copyQueue,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
        public void UpdateMemoryInfo()
        {
            var local = _adapter.QueryVideoMemoryInfo(
                0,
                MemorySegmentGroup.Local
            );

            var nonLocal = _adapter.QueryVideoMemoryInfo(
                0,
                MemorySegmentGroup.NonLocal
            );

            Info.VideoMemoryUsage = (ulong)local.CurrentUsage;
            Info.VideoMemoryBudget = (ulong)local.Budget;

            Info.SystemMemoryUsage = (ulong)nonLocal.CurrentUsage;
            Info.SystemMemoryBudget = (ulong)nonLocal.Budget;
        }
        public void Submit(ICommandList cmd)
        {

        }
        public void Dispose()
        {
            var debug = _device.QueryInterface<DeviceDebug>();

            debug.ReportLiveDeviceObjects(
                ReportingLevel.Detail);

            _deviceContext.Dispose();
            _device.Dispose();
        }
    }
}
