using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class ShaderPass : IDisposable
    {
        // Graphics stages
        public ShaderStage? Vertex { get; }
        public ShaderStage? Fragment { get; }

        // Compute stage
        public ShaderStage? Compute { get; }

        public bool IsCompute => Compute != null;

        public BlendStateDescription Blend;
        public DepthStencilState Depth;
        public RasterizerState Rasterizer;

        private readonly Dictionary<VertexInfo, IPipeline> graphicsPipelines = [];

        public IComputePipeline? ComputePipeline { get; set; }

        public IDescriptorLayout DescriptorLayout { get; internal set; } = null!;
        public IPipelineLayout PipelineLayout { get; set; } = null!; 
        public IPipeline Pipeline { get; set; } = null!;

        // Graphics constructor
        public ShaderPass(
            ShaderStage? vertex = null,
            ShaderStage? fragment = null,
            ShaderStage? compute = null)
        {
            Vertex = vertex;
            Fragment = fragment;
            Compute = compute;
        }

        public IPipeline GetPipeline(
            IGraphicsDevice device,
            VertexInfo vertexLayout)
        {
            if (IsCompute)
                throw new InvalidOperationException("Compute passes do not have graphics pipelines.");

            if (graphicsPipelines.TryGetValue(vertexLayout, out var pipeline))
                return pipeline;

            pipeline = device.CreateGraphicsPipeline(
                new GraphicsPipelineDescription
                {
                    VertexShader = Vertex!.GPU,
                    PixelShader = Fragment!.GPU,

                    VertexLayout = vertexLayout,

                    Blend = Blend,
                    DepthStencil = Depth,
                    Rasterizer = Rasterizer,

                    Topology = PrimitiveType.Triangles
                });

            graphicsPipelines[vertexLayout] = pipeline;

            return pipeline;
        }

        public IComputePipeline GetComputePipeline(IGraphicsDevice device)
        {
            if (!IsCompute)
                throw new InvalidOperationException("Graphics passes do not have compute pipelines.");

            ComputePipeline ??=
                device.CreateComputePipeline(new ComputePipelineDescription()
                {
                    ComputeShader = Compute!.GPU
                });

            return ComputePipeline;
        }

        public void Dispose()
        {
            foreach (var pipeline in graphicsPipelines.Values)
            {
                pipeline.Dispose();
            }

            graphicsPipelines.Clear();

            ComputePipeline?.Dispose();
        }
    }
}
