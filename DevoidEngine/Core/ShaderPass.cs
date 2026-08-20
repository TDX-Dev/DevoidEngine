using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class ShaderPass : IDisposable
    {
        private readonly IGraphicsDevice device;

        private readonly string? vertexPath;
        private readonly string? fragmentPath;
        private readonly string? computePath;

        private readonly Dictionary<
            ShaderDefineSet,
            ShaderVariant> variants = [];

        public bool IsCompute =>
            computePath != null;

        public BlendStateDescription Blend;
        public DepthStencilState Depth;
        public RasterizerState Rasterizer;

        public ShaderPass(
            IGraphicsDevice device,
            string? vertexPath,
            string? fragmentPath,
            string? computePath)
        {
            this.device = device;

            this.vertexPath = vertexPath;
            this.fragmentPath = fragmentPath;
            this.computePath = computePath;
        }

        public ShaderVariant GetVariant(IReadOnlyDictionary<string, string>? defines = null)
        {
            ShaderDefineSet key =
                new(defines);

            if (variants.TryGetValue(
                    key,
                    out ShaderVariant? variant))
            {
                return variant;
            }

            variant = CompileVariant(key);

            variants[key] = variant;

            return variant;
        }

        private ShaderVariant CompileVariant(ShaderDefineSet defines)
        {
            ShaderStage? vertex = null;
            ShaderStage? fragment = null;
            ShaderStage? compute = null;

            if (vertexPath != null)
            {
                vertex = new ShaderStage(
                    device.CreateShader(
                        new ShaderDescription
                        {
                            Name =
                                $"{Path.GetFileNameWithoutExtension(vertexPath)}_VS_{defines}",

                            FilePath = vertexPath,

                            Source =
                                File.ReadAllText(vertexPath),

                            EntryPoint = "VSMain",

                            Stage =
                                DevoidGPU.ShaderStage.Vertex,

                            Defines = defines.Defines.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal)
                        }));
            }

            if (fragmentPath != null)
            {
                fragment = new ShaderStage(
                    device.CreateShader(
                        new ShaderDescription
                        {
                            Name =
                                $"{Path.GetFileNameWithoutExtension(fragmentPath)}_FS_{defines}",

                            FilePath = fragmentPath,

                            Source =
                                File.ReadAllText(fragmentPath),

                            EntryPoint = "PSMain",

                            Stage =
                                DevoidGPU.ShaderStage.Fragment,

                            Defines = defines.Defines.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal)
                        }));
            }

            if (computePath != null)
            {
                compute = new ShaderStage(
                    device.CreateShader(
                        new ShaderDescription
                        {
                            Name =
                                $"{Path.GetFileNameWithoutExtension(computePath)}_CS_{defines}",

                            FilePath = computePath,

                            Source =
                                File.ReadAllText(computePath),

                            EntryPoint = "CSMain",

                            Stage =
                                DevoidGPU.ShaderStage.Compute,

                            Defines = defines.Defines.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal)
                        }));
            }

            List<ShaderReflectionData> reflections = [];

            if (vertex != null)
                reflections.Add(vertex.ShaderReflectionData);

            if (fragment != null)
                reflections.Add(fragment.ShaderReflectionData);

            if (compute != null)
                reflections.Add(compute.ShaderReflectionData);

            ShaderReflectionData reflection =
                ShaderReflectionData.Merge(
                    [.. reflections]);

            IDescriptorLayout descriptorLayout =
                CreateDescriptorLayout(
                    device,
                    reflection);

            return new ShaderVariant(
                vertex,
                fragment,
                compute,
                reflection,
                descriptorLayout);
        }

        public IPipeline GetPipeline(
            ShaderVariant variant,
            VertexInfo vertexLayout,
            PrimitiveType topology = PrimitiveType.Triangles)
        {
            return variant.GetPipeline(
                device,
                vertexLayout,
                topology,
                Blend,
                Depth,
                Rasterizer);
        }

        public IComputePipeline GetComputePipeline(
            ShaderVariant variant)
        {
            return variant.GetComputePipeline(device);
        }

        public IComputePipeline GetComputePipeline()
        {
            return GetVariant().GetComputePipeline(device);
        }

        private static IDescriptorLayout CreateDescriptorLayout(
            IGraphicsDevice device,
            ShaderReflectionData reflection)
        {
            List<DescriptorBinding> bindings = [];

            foreach (var buffer in reflection.UniformBuffers)
            {
                bindings.Add(
                    new DescriptorBinding
                    {
                        Binding = (uint)buffer.BindSlot,
                        Type = DescriptorType.UniformBuffer,
                        Stages = buffer.Stages
                    });
            }

            foreach (var resource in reflection.Resources)
            {
                DescriptorType type = resource.Type switch
                {
                    ShaderResourceType.Texture2D or
                    ShaderResourceType.TextureCube or
                    ShaderResourceType.Texture2DArray or
                    ShaderResourceType.Texture3D
                        => DescriptorType.Texture,

                    ShaderResourceType.Sampler
                        => DescriptorType.Sampler,

                    ShaderResourceType.StructuredBuffer
                        => DescriptorType.StorageBuffer,

                    ShaderResourceType.RWStructuredBuffer
                        => DescriptorType.RWStorageBuffer,

                    ShaderResourceType.RWTexture2D or
                    ShaderResourceType.RWTexture2DArray or
                    ShaderResourceType.RWTexture3D
                        => DescriptorType.RWTexture,

                    _ => throw new InvalidOperationException(
                        $"Unsupported shader resource type: {resource.Type}")
                };

                bindings.Add(
                    new DescriptorBinding
                    {
                        Binding = (uint)resource.BindSlot,
                        Type = type,
                        Stages = resource.Stage
                    });
            }

            return device.CreateDescriptorLayout(
                [.. bindings]);
        }

        public void Dispose()
        {
            foreach (ShaderVariant variant in variants.Values)
                variant.Dispose();

            variants.Clear();
        }
    }
}