namespace DevoidGPU
{
    public interface IPipelineLayout
    {
        IReadOnlyList<IDescriptorLayout> SetLayouts { get; }
    }
}
