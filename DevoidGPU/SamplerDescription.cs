namespace DevoidGPU
{
    public struct SamplerDescription
    {
        public FilterMode MinFilter;
        public FilterMode MagFilter;
        public FilterMode MipFilter;

        public WrapMode AddressU;
        public WrapMode AddressV;
        public WrapMode AddressW;

        public CompareFunc CompareFunc;

        public float MipLodBias;
        public float MinLod;
        public float MaxLod;

        public int MaxAnisotropy;
    }
}
