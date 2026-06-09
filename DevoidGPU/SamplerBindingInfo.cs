namespace DevoidGPU
{
    public class SamplerBindingInfo
    {
        public string Name { get; set; } = "";
        public int BindSlot { get; set; }
        public ShaderStage Stage { get; set; }
        public ShaderResourceType ResourceType { get; set; }
    }

}
