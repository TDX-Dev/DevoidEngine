using SharpDX.Direct3D;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using DeviceCreationFlags = SharpDX.Direct3D11.DeviceCreationFlags;
using DriverType = SharpDX.Direct3D.DriverType;

namespace DevoidGPU.DX11
{
    public sealed class DX11GraphicsDevice : IGraphicsDevice
    {
        private readonly Factory1 factory = null!;
        private readonly Device device = null!;
        private readonly DeviceContext deviceContext = null!;

        private readonly ICommandQueue graphicsQueue;
        private readonly ICommandQueue computeQueue;
        private readonly ICommandQueue copyQueue;

        public DX11GraphicsDevice()
        {
            factory = new Factory1();

            var levels = new[]
            {
                FeatureLevel.Level_11_0,
            };


            device = new Device(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport |
#if DEBUG
                DeviceCreationFlags.Debug
#endif
                , levels
            );

            deviceContext = device.ImmediateContext;

            // Queue Creation

            var queue = new DX11CommandQueue(deviceContext);

            graphicsQueue = queue;
            computeQueue = queue;
            copyQueue = queue;
        }

        public ISwapchain CreateSwapchain(SwapchainDescription desc)
        {
            return new DX11SwapChain(factory, device, desc);
        }

        public IShader CreateShader(ShaderDescription desc)
        {
            return new DX11Shader(device, desc);
        }

        public IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription desc)
        {
            DX11GraphicsPipeline dxPipeline = new()
            {
                VS = ((DX11Shader)desc.VertexShader).VS!,
                PS = ((DX11Shader)desc.VertexShader).PS!,

                Topology = DX11StateMapper.ToDXPrimitiveType(desc.Topology)
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

                    SourceBlend = DX11StateMapper.ToDXBlend(blendState.SrcColor),
                    DestinationBlend = DX11StateMapper.ToDXBlend(blendState.DstColor),
                    BlendOperation = DX11StateMapper.ToDXBlendOp(blendState.ColorOp),

                    SourceAlphaBlend = DX11StateMapper.ToDXBlend(blendState.SrcAlpha),
                    DestinationAlphaBlend = DX11StateMapper.ToDXBlend(blendState.DstAlpha),
                    AlphaBlendOperation = DX11StateMapper.ToDXBlendOp(blendState.AlphaOp),

                    RenderTargetWriteMask = DX11StateMapper.ToDXColorMask(blendState.WriteMask),
                };
            }

            dxPipeline.BlendState = new SharpDX.Direct3D11.BlendState(device, blendStateDesc);

            // Rasterizer State

            SharpDX.Direct3D11.RasterizerStateDescription rasterizerStateDesc = new()
            {
                CullMode = DX11StateMapper.ToDXCullMode(desc.Rasterizer.CullMode),
                FillMode = DX11StateMapper.ToDXFillMode(desc.Rasterizer.FillMode),
                IsFrontCounterClockwise = desc.Rasterizer.FrontCounterClockwise,

                IsScissorEnabled = desc.Rasterizer.EnableScissor
            };

            dxPipeline.RasterizerState = new SharpDX.Direct3D11.RasterizerState(device, rasterizerStateDesc);

            // Depth State

            SharpDX.Direct3D11.DepthStencilStateDescription depthStateDesc = new()
            {
                IsDepthEnabled = desc.DepthStencil.DepthTest != DepthTest.Disabled,
                DepthComparison = DX11StateMapper.ToDXDepthComparison(desc.DepthStencil.DepthFunc),
                DepthWriteMask = desc.DepthStencil.DepthWrite ? SharpDX.Direct3D11.DepthWriteMask.All : SharpDX.Direct3D11.DepthWriteMask.Zero
            };

            dxPipeline.DepthStencilState = new SharpDX.Direct3D11.DepthStencilState(device, depthStateDesc);

            // InputLayout

            var dxVS = (DX11Shader)desc.VertexShader;

            dxPipeline.InputLayout = new SharpDX.Direct3D11.InputLayout(
                device,
                dxVS.bytecode,
                DX11StateMapper.CreateInputElements(desc.VertexLayout)
            );

            return dxPipeline;
        }

        public IVertexBuffer CreateVertexBuffer(VertexBufferDescription desc)
        {
            return new DX11VertexBuffer(device, deviceContext, desc);
        }
        public IIndexBuffer CreateIndexBuffer(IndexBufferDescription desc)
        {
            return new DX11IndexBuffer(device, deviceContext, desc);
        }

        public ICommandList GetCommandList()
        {
            return new DX11CommandList(deviceContext);
        }
        public ICommandQueue GetCommandQueue(CommandListType type)
        {
            return type switch
            {
                CommandListType.Graphics => graphicsQueue,
                CommandListType.Compute => computeQueue,
                CommandListType.Copy => copyQueue,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public void Submit(ICommandList cmd)
        {

        }
    }
}
