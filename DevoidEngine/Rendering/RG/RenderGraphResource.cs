using DevoidEngine.Core;
using DevoidGPU;

namespace DevoidEngine.Rendering.RG
{
    internal sealed class RenderGraphResource
    {
        public RenderGraphHandle Handle;

        public string Name = string.Empty;

        public TextureDescription Description;

        public Texture? PhysicalTexture;

        public bool Imported;

        public bool Transient;

        public uint FirstUse = uint.MaxValue;

        public uint LastUse;

        public bool IsAllocated => PhysicalTexture is not null;
    }
}
