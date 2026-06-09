namespace DevoidGPU
{
    public class ShaderReflectionData
    {
        public List<UniformBufferInfo> UniformBuffers { get; } = [];
        public List<ShaderResourceInfo> Resources { get; } = [];
        public List<TextureBindingInfo> TextureBindings { get; } = [];
        public List<SamplerBindingInfo> SamplerBindings { get; } = [];
        public List<InputParameterInfo> InputParameters { get; } = [];

        public int GetUniformBufferSlot(string name)
        {
            var buffers = UniformBuffers;
            for (int j = 0; j < buffers.Count; j++)
            {
                var buffer = buffers[j];
                if (string.Equals(buffer.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return j;
                }
            }
            return -1;
        }

        public static ShaderReflectionData Merge(
            params ShaderReflectionData[] reflections
        )
        {
            ShaderReflectionData result = new();

            HashSet<string> resources = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> inputs = new(StringComparer.OrdinalIgnoreCase);

            foreach (ShaderReflectionData reflection in reflections)
            {
                foreach (UniformBufferInfo buffer in reflection.UniformBuffers)
                {
                    AddUniformBuffer(result, buffer);
                }

                foreach (ShaderResourceInfo resource in reflection.Resources)
                {
                    if (resources.Add(resource.Name))
                    {
                        result.Resources.Add(resource);
                    }
                }

                foreach (TextureBindingInfo texture in reflection.TextureBindings)
                {
                    AddTexture(result, texture);
                }

                foreach (SamplerBindingInfo sampler in reflection.SamplerBindings)
                {
                    AddSampler(result, sampler);
                }

                foreach (InputParameterInfo input in reflection.InputParameters)
                {
                    if (inputs.Add(input.SemanticName))
                    {
                        result.InputParameters.Add(input);
                    }
                }
            }

            return result;
        }

        private static void AddUniformBuffer(
    ShaderReflectionData result,
    UniformBufferInfo buffer)
        {
            UniformBufferInfo? existing =
                result.UniformBuffers
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Name,
                            buffer.Name,
                            StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                result.UniformBuffers.Add(buffer);
                return;
            }

            if (existing.BindSlot != buffer.BindSlot)
            {
                throw new Exception(
                    $"Uniform buffer '{buffer.Name}' uses different slots.");
            }
        }

        private static void AddTexture(
    ShaderReflectionData result,
    TextureBindingInfo texture)
        {
            TextureBindingInfo? existing =
                result.TextureBindings
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Name,
                            texture.Name,
                            StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                result.TextureBindings.Add(texture);
                return;
            }

            if (existing.BindSlot != texture.BindSlot)
            {
                throw new Exception(
                    $"Texture '{texture.Name}' uses different slots.");
            }
        }

        private static void AddSampler(
    ShaderReflectionData result,
    SamplerBindingInfo sampler)
        {
            SamplerBindingInfo? existing =
                result.SamplerBindings
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Name,
                            sampler.Name,
                            StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                result.SamplerBindings.Add(sampler);
                return;
            }

            if (existing.BindSlot != sampler.BindSlot)
            {
                throw new Exception(
                    $"Sampler '{sampler.Name}' uses different slots.");
            }
        }
    }
}
