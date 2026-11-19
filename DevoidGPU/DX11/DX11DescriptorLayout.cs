namespace DevoidGPU.DX11
{
    internal sealed class Dx11DescriptorLayout : IDescriptorLayout
    {
        public DescriptorBinding[] Bindings { get; }

        public Dx11DescriptorLayout(DescriptorBinding[] bindings)
        {
            Bindings = bindings;
        }
    }
}
