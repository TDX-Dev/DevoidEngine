using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class Sampler : IDisposable
    {
        private readonly ISampler _gpuSampler;

        public ISampler Gpu => _gpuSampler;
        private static Sampler @default = Create(new SamplerDescription()
        {
            AddressU = WrapMode.Repeat,
            AddressV = WrapMode.Repeat,
            AddressW = WrapMode.Repeat,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MipFilter = FilterMode.Linear,
            MinLod = 0f,
            MaxLod = float.MaxValue,
            MaxAnisotropy = 1,
        });

        public SamplerDescription Description { get; }
        public static Sampler Default { get => @default; set => @default = value; }

        public Sampler(
            IGraphicsDevice device,
            SamplerDescription description)
        {
            Description = description;
            _gpuSampler = device.CreateSampler(description);
        }

        public static Sampler Create(
            SamplerDescription description)
        {
            return new Sampler(
                Engine.GraphicsDevice,
                description);
        }

        public void Dispose()
        {
            _gpuSampler.Dispose();
        }
    }
}
