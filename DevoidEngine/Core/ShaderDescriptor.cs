using System.Text.Json.Serialization;

namespace DevoidEngine.Core
{

    public class ShaderDescriptor
    {
        public string Name { get; set; } = string.Empty;
        public string DefaultPass { get; set; } = string.Empty;
        public MaterialParameterDescriptor? MaterialParameters { get; set; }
        public List<ShaderResourceDescriptor> Resources { get; set; } = [];
        public List<PassDescriptor> Passes { get; set; } = [];
    }

    public class PassDescriptor
    {
        public string Name { get; set; } = string.Empty;

        public ShaderFiles Shaders { get; set; } = new();

        public StateDescriptor? States { get; set; }
    }

    public class MaterialParameterDescriptor
    {
        public string BufferName { get; set; } = string.Empty;
    }

    public class ShaderResourceDescriptor
    {
        public string Name { get; set; } = string.Empty;

        public ShaderResourceKind Kind { get; set; }

        public ShaderTextureType? TextureType { get; set; }

        public string? DisplayName { get; set; }

        public string? Default { get; set; }
    }

    public class ShaderFiles
    {
        public string VS { get; set; } = string.Empty;

        public string FS { get; set; } = string.Empty;
        public string CS { get; set; } = string.Empty;
    }

    public class StateDescriptor
    {
        public string? Blend { get; set; }

        public string? Depth { get; set; }

        public string? Cull { get; set; }
    }


    public enum ShaderResourceKind
    {
        SampledTexture,
        StorageTexture,
        UniformBuffer,
        StorageBuffer,
        Sampler
    }

    public enum ShaderTextureType
    {
        Texture2D,
        TextureCube,
        Texture2DArray,
        Texture3D
    }
}
