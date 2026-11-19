using DevoidGPU;

namespace DevoidEngine.Serialization
{
    public class MaterialDescriptor
    {
        public string? Name { get; set; } = string.Empty;
        public MaterialPassDescriptor? Pass { get; set; }
    }

    public class MaterialPassDescriptor
    {
        public string? Name { get; set; } = string.Empty;
        public ShaderProgramsDescriptor ShaderPrograms { get; set; } = new ShaderProgramsDescriptor();
        public PipelineStateDescriptor PipelineState { get; set; } = new PipelineStateDescriptor();
        public Dictionary<string, UniformDefinitionDescriptor> Uniforms { get; set; } = [];
    }

    public class ShaderProgramsDescriptor
    {
        public string? Vs { get; set; } = string.Empty;
        public string? Fs { get; set; } = string.Empty;
    }

    public class PipelineStateDescriptor
    {
        public BlendMode? Blend { get; set; }
        public DepthTest? DepthCompareMode { get; set; }
        public bool? WriteDepth { get; set; }
        public CullMode? CullMode { get; set; }
        public FillMode? FillMode { get; set; }
        public PrimitiveType? PrimitiveType { get; set; }
    }
    public class MaterialUniformsDescriptor { }
    public class UniformDefinitionDescriptor
    {
        public UniformResourceType Type { get; set; } = UniformResourceType.None;
        public List<UniformValue>? Values { get; set; } = [];
        public string? Value { get; set; } = string.Empty;
    }

    public class UniformValue
    {
        public string Name { get; set; } = string.Empty;
        public UniformValueType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }


    public enum UniformResourceType
    {
        None,
        ConstantBuffer,
        TextureResource
    }

    public enum UniformValueType
    {
        Int,
        Float,
        Color,
        Vector2,
        Vector3,
        Vector4,
        Matrix4X4
    }
}
