namespace DevoidGPU
{
    public class ShaderResourceInfo
    {
        public string Name { get; init; } = "";

        public ShaderResourceType Type { get; init; }

        public int BindSlot { get; init; }

        public ShaderStage Stage { get; set; }

        public int ArraySize { get; init; }

        public bool ReadWrite { get; init; }
    }
}
