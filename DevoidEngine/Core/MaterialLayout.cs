using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class MaterialLayout
    {
        public string BufferName = "";

        public int BufferSize;

        public int BufferBindSlot;

        public Dictionary<string, ShaderVariableInfo> Variables = [];

        public Dictionary<string, TextureBindingInfo> Textures = [];
        public Dictionary<string, SamplerBindingInfo> Samplers = [];


        public Dictionary<string, StorageTextureBindingInfo> StorageTextures { get; } = [];

        public Dictionary<string, StorageBufferBindingInfo> StorageBuffers { get; } = [];
    }
}
