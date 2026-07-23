using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public sealed class Sampler : IDisposable
    {
        private readonly ISampler gpuSampler;

        public ISampler GPU => gpuSampler;
        public static Sampler Default => Create(new SamplerDescription()
        {
            AddressU = WrapMode.ClampToEdge,
            AddressV = WrapMode.ClampToEdge,
            AddressW = WrapMode.ClampToEdge,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MipFilter = FilterMode.Linear,
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
