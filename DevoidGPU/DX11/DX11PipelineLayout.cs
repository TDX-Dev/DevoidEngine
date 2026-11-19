namespace DevoidGPU.DX11
{
    internal sealed class Dx11PipelineLayout : IPipelineLayout
    {
        public IReadOnlyList<IDescriptorLayout> SetLayouts { get; }

        public Dx11PipelineLayout(IDescriptorLayout[] setLayouts)
        {
            SetLayouts = setLayouts;
        }
    }
}
