namespace DevoidGPU.DX11
{
    internal sealed class DX11DescriptorLayout : IDescriptorLayout
    {
        public DescriptorBinding[] Bindings { get; }

        public DX11DescriptorLayout(DescriptorBinding[] bindings)
        {
            Bindings = bindings;
        }
    }
}
