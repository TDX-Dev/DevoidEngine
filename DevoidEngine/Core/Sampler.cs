using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class Sampler : IDisposable
    {
        private readonly ISampler gpuSampler;

        public ISampler GPU => gpuSampler;
        public static Sampler Default => Create(new SamplerDescription()
        {
            AddressU = WrapMode.MirrorRepeat,
            AddressV = WrapMode.MirrorRepeat,
            AddressW = WrapMode.MirrorRepeat,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MipFilter = FilterMode.Linear,
            MinLOD = 0f,
            MaxLOD = float.MaxValue,
            MaxAnisotropy = 1,
        });

        public SamplerDescription Description { get; }

        public Sampler(
            IGraphicsDevice device,
            SamplerDescription description)
        {
            Description = description;
            gpuSampler = device.CreateSampler(description);
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
            gpuSampler.Dispose();
        }
    }
}
