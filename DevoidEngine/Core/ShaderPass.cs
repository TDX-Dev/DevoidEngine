using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class ShaderPass : IDisposable
    {
        public IShader VS { get; }
        public IShader FS { get; }
        public IDescriptorLayout DescriptorLayout { get; }
        public ShaderReflectionData ShaderReflectionData { get; }


        public ShaderPass(IShader vertexShader, IShader fragmentShader)
        {
            VS = vertexShader;
            FS = fragmentShader;

            ShaderReflectionData = ShaderReflectionData.Merge([VS.ReflectionData, FS.ReflectionData]);
            IDescriptorLayout descriptorLayout = CreateDescriptorLayout(Engine.GraphicsDevice, ShaderReflectionData);
            DescriptorLayout = descriptorLayout;
        }

        IDescriptorLayout CreateDescriptorLayout(IGraphicsDevice gd, ShaderReflectionData reflectionData)
        {
            List<DescriptorBinding> bindings = [];

            foreach (var buffer in reflectionData.UniformBuffers)
            {
                bindings.Add(
                    new DescriptorBinding()
                    {
                        Binding = (uint)buffer.BindSlot,
                        Type = DescriptorType.UniformBuffer,
                        Stages = buffer.Stages
                    }
                );
            }

            foreach (var resource in reflectionData.Resources)
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

                    ShaderResourceType.RwStructuredBuffer
                        => DescriptorType.RwStorageBuffer,

                    ShaderResourceType.RwTexture2D or
                    ShaderResourceType.RwTexture2DArray or
                    ShaderResourceType.RwTexture3D
                        => DescriptorType.RwTexture,

                    _ => throw new InvalidOperationException(
                        $"Unsupported shader resource type: {resource.Type}")
                };

                bindings.Add(
                    new DescriptorBinding
                    {
                        Binding = (uint)resource.BindSlot,
                        Type = type,
                        Stages = resource.Stage
                    }
                );
            }

            return gd.CreateDescriptorLayout([.. bindings]);
        }

        public void Dispose()
        {
            VS.Dispose();
            FS.Dispose();
        }
    }
}
