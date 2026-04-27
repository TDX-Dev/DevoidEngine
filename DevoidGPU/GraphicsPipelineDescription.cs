using SharpDX.Direct3D;

namespace DevoidGPU
{
    public struct GraphicsPipelineDescription
    {
        public IShader VertexShader;
        public IShader PixelShader;

        public PrimitiveTopology Topology;

        public RasterizerState Rasterizer;
        public DepthStencilState DepthStencil;
        public BlendStateDescription Blend;
    }
}
