using SharpDX.Direct3D11;


namespace DevoidGPU.DX11
{
    internal class DX11GraphicsPipeline : IPipeline
    {
        public VertexShader VS = null!;
        public PixelShader PS = null!;

        public SharpDX.Direct3D.PrimitiveTopology Topology;
        public SharpDX.Direct3D11.RasterizerState RasterizerState = null!;
        public SharpDX.Direct3D11.DepthStencilState DepthStencilState = null!;
        public SharpDX.Direct3D11.BlendState BlendState = null!;
    }
}
