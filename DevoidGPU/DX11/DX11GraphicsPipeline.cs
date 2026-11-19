using SharpDX.Direct3D11;


namespace DevoidGPU.DX11
{
    internal class Dx11GraphicsPipeline : IPipeline
    {
        public VertexShader Vs = null!;
        public PixelShader Ps = null!;

        public InputLayout InputLayout = null!;

        public SharpDX.Direct3D.PrimitiveTopology Topology;
        public SharpDX.Direct3D11.RasterizerState RasterizerState = null!;
        public SharpDX.Direct3D11.DepthStencilState DepthStencilState = null!;
        public SharpDX.Direct3D11.BlendState BlendState = null!;

        public Dx11PipelineLayout Layout = null!;

        public void Dispose()
        {
            RasterizerState?.Dispose();
            DepthStencilState?.Dispose();
            BlendState?.Dispose();
        }
    }
}
