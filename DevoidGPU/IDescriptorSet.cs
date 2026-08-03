namespace DevoidGPU
{
    public interface IDescriptorSet
    {
        IDescriptorLayout Layout { get; }

        void SetShaderStorageBuffer(uint binding, IShaderStorageBuffer buffer);
        void SetUniformBuffer(uint binding, IUniformBuffer buffer);
        void SetTexture(uint binding, ITexture texture);
        void SetSampler(uint binding, ISampler sampler);
        public void SetRWTexture(uint binding, ITexture texture);
        void SetRWShaderStorageBuffer(uint binding, IShaderStorageBuffer buffer);


    }
}
