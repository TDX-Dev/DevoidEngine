namespace DevoidGPU
{
    public class UniformBufferInfo
    {
        public string Name = "";
        public int Size;
        public int BindSlot;
        public List<ShaderVariableInfo> Variables = [];
    }
}
