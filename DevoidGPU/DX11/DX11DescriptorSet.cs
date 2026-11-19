namespace DevoidGPU.DX11
{
    internal sealed class Dx11DescriptorSet : IDescriptorSet
    {

        public IDescriptorLayout Layout { get; }

        internal readonly Dictionary<uint, IShaderStorageBuffer> StorageBuffers = [];
        internal readonly Dictionary<uint, IUniformBuffer> UniformBuffers = [];
        internal readonly Dictionary<uint, ITexture> Textures = [];
        internal readonly Dictionary<uint, ISampler> Samplers = [];

        internal readonly Dictionary<uint, IShaderStorageBuffer> RwStorageBuffers = [];
        internal readonly Dictionary<uint, ITexture> RwTextures = [];

        public Dx11DescriptorSet(IDescriptorLayout layout)
        {
            Layout = layout;
        }

        public void SetShaderStorageBuffer(
            uint binding,
            IShaderStorageBuffer buffer
        )
        {
            StorageBuffers[binding] = buffer;
        }

        public void SetUniformBuffer(
            uint binding,
            IUniformBuffer buffer)
        {
            UniformBuffers[binding] = buffer;
        }

        public void SetTexture(
            uint binding,
            ITexture texture)
        {
            Textures[binding] = texture;
        }

        public void SetSampler(
            uint binding,
            ISampler sampler)
        {
            Samplers[binding] = sampler;
        }

        public void SetRWShaderStorageBuffer(
    uint binding,
    IShaderStorageBuffer buffer)
        {
            RwStorageBuffers[binding] = buffer;
        }

        public void SetRwTexture(
            uint binding,
            ITexture texture)
        {
            RwTextures[binding] = texture;
        }
    }
}
