namespace DevoidGPU
{
    public class TextureBindingInfo
    {
        public string Name { get; set; } = "";
        public int BindSlot { get; set; }
        public ShaderStage Stage { get; set; }
        public ShaderResourceType ResourceType { get; set; }
        public int ArraySize { get; set; } = 1;
    }

}
