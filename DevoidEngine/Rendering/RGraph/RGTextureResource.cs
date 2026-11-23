using DevoidEngine.Core;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph
{
    public struct RGTextureResource
    {
        public string Name;
        public TextureDescription Description;
        public Texture? Resource;
        public bool Imported;
        public bool Allocated;
    }
}
