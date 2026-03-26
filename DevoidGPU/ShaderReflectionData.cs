namespace DevoidGPU
{
    public class ShaderVariableInfo
    {
        public string Name;
        public int Offset;
        public int Size;
        public ShaderVariableType Type;
    }

    public class InputParameterInfo
    {
        public string SemanticName;
        public int SemanticIndex;
    }

    public class UniformBufferInfo
    {
        public string Name;
        public int Size;
        public int BindSlot;
        public List<ShaderVariableInfo> Variables = new();
    }

    public class ShaderResourceInfo
    {
        public string Name;
        public int BindSlot;
        public ShaderResourceType ResourceType;
    }

    public class TextureBindingInfo
    {
        public string Name { get; set; }
        public int BindSlot { get; set; }
        public ShaderStage Stage { get; set; }
        public ShaderResourceType ResourceType { get; set; }
        public int ArraySize { get; set; } = 1;
    }

    public class ShaderReflectionData
    {
        public List<UniformBufferInfo> UniformBuffers { get; } = new();
        public List<ShaderResourceInfo> Resources { get; } = new();
        public List<TextureBindingInfo> TextureBindings { get; } = new();
        public List<InputParameterInfo> InputParameters { get; } = new();

        public int GetUniformBufferSlot(string name)
        {
            var buffers = UniformBuffers;
            for (int j = 0; j < buffers.Count; j++)
            {
                var buffer = buffers[j];
                if (string.Equals(buffer.Name, "PerObject", StringComparison.OrdinalIgnoreCase))
                {
                    return j;
                }
            }
            return -1;
        }
    }

    public enum ShaderResourceType
    {
        Texture2D,
        Texture3D,
        TextureCube,
        Sampler,
        StructuredBuffer,
        RWStructuredBuffer
    }

    public enum ShaderVariableType
    {
        Float,
        Int,
        Vector4,
        Vector3,
        Vector2,
        Matrix3x3,
        Matrix4x4,
        Custom
    }
}