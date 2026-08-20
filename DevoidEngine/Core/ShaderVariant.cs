using DevoidGPU;

namespace DevoidEngine.Core
{
    public readonly record struct PipelineKey(VertexInfo VertexLayout, PrimitiveType Topology);
    public sealed class ShaderVariant : IDisposable
    {
        public ShaderStage? Vertex { get; }
        public ShaderStage? Fragment { get; }
        public ShaderStage? Compute { get; }

        public ShaderReflectionData ReflectionData { get; }

        public IDescriptorLayout DescriptorLayout { get; }

        private readonly Dictionary<PipelineKey, IPipeline>
            graphicsPipelines = [];

        private IComputePipeline? computePipeline;

        public bool IsCompute => Compute != null;

        public ShaderVariant(
            ShaderStage? vertex,
            ShaderStage? fragment,
            ShaderStage? compute,
            ShaderReflectionData reflectionData,
            IDescriptorLayout descriptorLayout)
        {
            Vertex = vertex;
            Fragment = fragment;
            Compute = compute;

            ReflectionData = reflectionData;
            DescriptorLayout = descriptorLayout;
        }

        public IPipeline GetPipeline(
            IGraphicsDevice device,
            VertexInfo vertexLayout,
            PrimitiveType topology,
            BlendStateDescription blend,
            DepthStencilState depth,
            RasterizerState rasterizer)
        {
            if (IsCompute)
                throw new InvalidOperationException(
                    "Compute variants do not have graphics pipelines.");

            PipelineKey key = new(vertexLayout, topology);

            if (graphicsPipelines.TryGetValue(
                    key,
                    out IPipeline? pipeline))
            {
                return pipeline;
            }

            pipeline = device.CreateGraphicsPipeline(
                new GraphicsPipelineDescription
                {
                    VertexShader = Vertex!.GPU,
                    PixelShader = Fragment!.GPU,

                    VertexLayout = vertexLayout,
                    Topology = topology,

                    Blend = blend,
                    DepthStencil = depth,
                    Rasterizer = rasterizer
                });

            graphicsPipelines[key] = pipeline;

            return pipeline;
        }

        public IComputePipeline GetComputePipeline(
            IGraphicsDevice device)
        {
            if (!IsCompute)
                throw new InvalidOperationException(
                    "Graphics variants do not have compute pipelines.");

            computePipeline ??=
                device.CreateComputePipeline(
                    new ComputePipelineDescription
                    {
                        ComputeShader = Compute!.GPU
                    });

            return computePipeline;
        }

        public void Dispose()
        {
            foreach (IPipeline pipeline in graphicsPipelines.Values)
                pipeline.Dispose();

            graphicsPipelines.Clear();

            computePipeline?.Dispose();
        }
    }
}