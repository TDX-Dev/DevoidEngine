using SharpDX.Direct3D11;
using System.Numerics;

namespace DevoidGPU.DX11
{
    internal sealed class DX11CommandList : ICommandList
    {
        public CommandListType Type { get; }

    
        private readonly DeviceContext deviceContext;

        // binding cache;
        private DX11Framebuffer? currentFramebuffer;
        private (int, int, int, int) currentViewport;


        public DX11CommandList(DeviceContext context) { deviceContext = context; }

        public void Begin()
        {
            currentFramebuffer = null;
            currentViewport = default;

            // optional safety reset
            deviceContext.InputAssembler.InputLayout = null;
        }

        public void End() { /* No Op */ }

        public void SetViewport(int x, int y, int width, int height)
        {
            if ((x, y, width, height) == currentViewport)
                return;
            deviceContext.Rasterizer.SetViewport(x, y, width, height);
            currentViewport = (x, y, width, height);
        }

        public void SetScissor(int x, int y, int width, int height)
        {
            deviceContext.Rasterizer.SetScissorRectangle(x, y, width, height);
        }

        public void SetFramebuffer(IFrameBuffer framebuffer)
        {
            DX11Framebuffer dx11Fb = (DX11Framebuffer)framebuffer;
            if (ReferenceEquals(currentFramebuffer, dx11Fb))
                return;

            currentFramebuffer = dx11Fb;

            deviceContext.OutputMerger.SetRenderTargets(dx11Fb.DSV, dx11Fb.RTVs);
        }

        public void ClearColor(int attachmentIndex, Vector4 color)
        {
            if (currentFramebuffer == null)
                throw new InvalidOperationException("Framebuffer not bound");

            if (attachmentIndex < 0 || attachmentIndex >= currentFramebuffer.RTVs.Length)
                throw new ArgumentOutOfRangeException(nameof(attachmentIndex));

            deviceContext.ClearRenderTargetView(currentFramebuffer.RTVs[attachmentIndex], new SharpDX.Mathematics.Interop.RawColor4(color.X, color.Y, color.Z, color.W));
        }

        public void ClearDepthStencil(float depth, byte stencil)
        {
            if (currentFramebuffer?.DSV == null)
                throw new InvalidOperationException("No depth stencil bound.");

            deviceContext.ClearDepthStencilView(
                currentFramebuffer.DSV,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                depth,
                stencil
            );
        }
        public void SetPipeline(IPipeline pipeline)
        {
            var p = (DX11GraphicsPipeline)pipeline;

            deviceContext.InputAssembler.PrimitiveTopology = p.Topology;
            deviceContext.InputAssembler.InputLayout = p.InputLayout;

            deviceContext.VertexShader.Set(p.VS);
            deviceContext.PixelShader.Set(p.PS);

            deviceContext.Rasterizer.State = p.RasterizerState;
            deviceContext.OutputMerger.SetBlendState(p.BlendState);
            deviceContext.OutputMerger.SetDepthStencilState(p.DepthStencilState);
        }
        public void SetVertexBuffer(IVertexBuffer buffer)
        {
            var vb = (DX11VertexBuffer)buffer;

            deviceContext.InputAssembler.SetVertexBuffers(
                vb.Slot,
                new VertexBufferBinding(vb.Buffer, vb.Stride, 0)
            );
        }

        public void SetIndexBuffer(IIndexBuffer buffer)
        {
            var ib = (DX11IndexBuffer)buffer;

            deviceContext.InputAssembler.SetIndexBuffer(
                ib.Buffer,
                DX11StateMapper.ToDXIndexFormat(buffer.Format),
                0
            );
        }

        public void Draw(int vertexCount, int startVertexLocation)
        {
            deviceContext.Draw(vertexCount, startVertexLocation);
        }
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }
    }
}
