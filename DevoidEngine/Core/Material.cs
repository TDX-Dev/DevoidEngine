using DevoidEngine.Logging;
using DevoidEngine.Serialization;
using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public readonly record struct PipelineKey(VertexInfo VertexLayout, PrimitiveType Topology);
    public sealed class Material : IDisposable
    {
        public string Name { get; }
        public MaterialPass Pass { get; } = null!;
        private readonly Dictionary<PipelineKey, IPipeline> _graphicsPipelines = [];

        public Material(MaterialDescriptor descriptor)
        {
            Name = descriptor.Name ?? "Unnamed";

            MaterialPassDescriptor? pass = descriptor.Pass;

            if (pass == null)
            {
                DevoidLog.Error(LogCategory.Core, "Pass is not defined in Material definition.");
                return;
            }

            string? shaderVertexPath = pass.ShaderPrograms.Vs;
            string? shaderFragmentPath = pass.ShaderPrograms.Fs;

            if (shaderVertexPath == null)
            {
                DevoidLog.Error(LogCategory.Core, "Vertex path in material definition is null");
                throw new InvalidDataException("Vertex path in material definition is null");
            }

            if (shaderFragmentPath == null)
            {
                DevoidLog.Error(LogCategory.Core, "Fragment path in material definition is null");
                throw new InvalidDataException("Fragment path in material definition is null");
            }

            Shader passShader = new(Name, shaderVertexPath, shaderFragmentPath);

            Pass = new MaterialPass(passShader, pass.PipelineState, pass.Uniforms);


            // Material Pass is a left over abstraction from when the engine supported multiple material passes, just leaving it here since it may come in handy later.
        }
        public IPipeline GetPipeline(VertexInfo vertexLayout, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            PipelineKey key = new(vertexLayout, primitiveType);

            if (_graphicsPipelines.TryGetValue(key, out var pipeline))
            {
                return pipeline;
            }

            pipeline = Engine.GraphicsDevice.CreateGraphicsPipeline(PipelineHelper.CreatePipelineDescription(Pass.PipelineState, Pass.Shader.GetActivePass(), vertexLayout, primitiveType));
            return pipeline;
        }
        public Shader GetActiveShader()
        {
            return Pass.Shader;
        }
        public void Dispose()
        {
            Pass.Dispose();
            _graphicsPipelines.Clear();
        }
    }
}
