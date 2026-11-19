namespace DevoidEngine.Rendering.RG
{
    public readonly record struct RenderGraphHandle(uint Id)
    {
        public bool IsValid => Id != uint.MaxValue;
        public static RenderGraphHandle Invalid => new(uint.MaxValue);
    }
}
