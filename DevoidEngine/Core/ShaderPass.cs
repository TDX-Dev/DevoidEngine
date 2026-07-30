using DevoidGPU;

namespace DevoidEngine.Core
{
    public class ShaderPass
    {

        public readonly ShaderStage Vertex;
        public readonly ShaderStage Fragment;

        public BlendStateDescription Blend;
        public DepthStencilState Depth;
        public RasterizerState Rasterizer;

        private readonly Dictionary<VertexInfo, IPipeline> pipelines = [];
        public IDescriptorLayout DescriptorLayout { get; internal set; } = null!;
        public IPipelineLayout PipelineLayout { get; set; } = null!;
        public IPipeline Pipeline { get; set; } = null!;

        public ShaderPass(ShaderStage vertex, ShaderStage fragment)
        {
            Vertex = vertex;
            Fragment = fragment;
        }

        public IPipeline GetPipeline(
            IGraphicsDevice device,
            VertexInfo vertexLayout
        )
        {
            if (pipelines.TryGetValue(vertexLayout, out var pipeline))
                return pipeline;

            pipeline = device.CreateGraphicsPipeline(
                new GraphicsPipelineDescription
                {
                    VertexShader = Vertex.GPU,
                    PixelShader = Fragment.GPU,

                    VertexLayout = vertexLayout,

                    Blend = Blend,
                    DepthStencil = Depth,
                    Rasterizer = Rasterizer,

                    Topology = PrimitiveType.Triangles
                });

            pipelines[vertexLayout] = pipeline;

            return pipeline;
        }
    }
}
