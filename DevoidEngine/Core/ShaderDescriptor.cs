using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class ShaderDescriptor
    {
        public string Name { get; set; } = string.Empty;
        public MaterialParameterDescriptor? MaterialParameters { get; set; }
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

        public List<MaterialTextureDescriptor> Textures { get; set; } = [];
    }

    public class MaterialTextureDescriptor
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = "Texture2D";

        public string? DisplayName { get; set; }

        public string? Default { get; set; }
    }

    public class ShaderFiles
    {
        public string VS { get; set; } = string.Empty;

        public string FS { get; set; } = string.Empty;
    }

    public class StateDescriptor
    {
        public string? Blend { get; set; }

        public string? Depth { get; set; }

        public string? Cull { get; set; }
    }
}
