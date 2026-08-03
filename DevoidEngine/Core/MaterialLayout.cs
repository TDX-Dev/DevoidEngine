using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class MaterialLayout
    {
        public string BufferName = "";

        public int BufferSize;

        public int BufferBindSlot;

        public Dictionary<string, ShaderVariableInfo> Variables = [];

        public Dictionary<string, ShaderResourceInfo> Textures = [];
        public Dictionary<string, ShaderResourceInfo> Samplers = [];
    }
}
