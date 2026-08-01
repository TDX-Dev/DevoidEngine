namespace DevoidGPU.DX11
{
    internal sealed class DX11DescriptorSet : IDescriptorSet
    {

        public IDescriptorLayout Layout { get; }

        internal readonly Dictionary<uint, IShaderStorageBuffer> storageBuffers = [];
        internal readonly Dictionary<uint, IUniformBuffer> uniformBuffers = [];
        internal readonly Dictionary<uint, ITexture> textures = [];
        internal readonly Dictionary<uint, ISampler> samplers = [];

        internal readonly Dictionary<uint, IShaderStorageBuffer> rwStorageBuffers = [];
        internal readonly Dictionary<uint, ITexture> rwTextures = [];

        public DX11DescriptorSet(IDescriptorLayout layout)
        {
            Layout = layout;
        }

        public void SetShaderStorageBuffer(
            uint binding,
            IShaderStorageBuffer buffer
        )
        {
            storageBuffers[binding] = buffer;
        }

        public void SetUniformBuffer(
            uint binding,
            IUniformBuffer buffer)
        {
            uniformBuffers[binding] = buffer;
        }

        public void SetTexture(
            uint binding,
            ITexture texture)
        {
            textures[binding] = texture;
        }

        public void SetSampler(
            uint binding,
            ISampler sampler)
        {
            samplers[binding] = sampler;
        }

        public void SetRWShaderStorageBuffer(
    uint binding,
    IShaderStorageBuffer buffer)
        {
            rwStorageBuffers[binding] = buffer;
        }

        public void SetRWTexture(
            uint binding,
            ITexture texture)
        {
            rwTextures[binding] = texture;
        }
    }
}
