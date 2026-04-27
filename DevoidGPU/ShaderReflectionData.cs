namespace DevoidGPU
{
    public class ShaderReflectionData
    {
        public List<UniformBufferInfo> UniformBuffers { get; } = [];
        public List<ShaderResourceInfo> Resources { get; } = [];
        public List<TextureBindingInfo> TextureBindings { get; } = [];
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
    }
}
