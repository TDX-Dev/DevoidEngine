namespace DevoidGPU
{
    public struct DescriptorBinding
    {
        public uint Binding;
        public DescriptorType Type;
        public ShaderStage Stages;

        public override readonly string ToString()
        {
            return $"Binding={Binding}, Type={Type}, Stages={Stages}";
        }
    }
}
