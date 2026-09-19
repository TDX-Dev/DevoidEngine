using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Rendering.PostProcessing
{
    public sealed class BloomUtility
    {
        public enum MipMode
        {
            Normal,
            Vertical,
            Horizontal
        }

        private readonly struct BloomMip
        {
            public readonly Vector2 Size;
            public readonly Texture Texture;

            public BloomMip(Vector2 size, Texture texture)
            {
                Size = size;
                Texture = texture;
            }
        }

        private struct BloomMipShaderData
        {
            public Vector2 mipSize;
            public float filterRadius;
        }

        private readonly MaterialInstance prefilterMaterial;
        private readonly MaterialInstance downsampleMaterial;
        private readonly MaterialInstance upsampleMaterial;

        private readonly RenderTarget target;
        private readonly UniformBuffer mipShaderDataBuffer;
        private readonly Sampler downsampleBloomSampler;
        private readonly Sampler upsampleBloomSampler;

        private readonly List<BloomMip> bloomMipList = [];
        private readonly List<BloomMip> upsampleMipList = [];

        public BloomUtility()
        {
            this.prefilterMaterial = new(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_process_pass.dsd"))));
            this.downsampleMaterial = new(new(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_downsample_pass.dsd"))));
            this.upsampleMaterial = new(new(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_upsample_pass.dsd"))));

            target = RenderTarget.Create(1);

            mipShaderDataBuffer = new UniformBuffer(
                Engine.GraphicsDevice,
                ResourceUsage.Dynamic,
                (uint)Unsafe.SizeOf<BloomMipShaderData>());

            downsampleBloomSampler = Sampler.Create(new SamplerDescription
            {
                AddressU = WrapMode.ClampToBorder,
                AddressV = WrapMode.ClampToBorder,
                AddressW = WrapMode.ClampToBorder,
                MagFilter = FilterMode.Linear,
                MinFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                MinLOD = 0f,
                MaxLOD = float.MaxValue,
                MaxAnisotropy = 1
            });

            upsampleBloomSampler = Sampler.Create(new SamplerDescription
            {
                AddressU = WrapMode.ClampToEdge,
                AddressV = WrapMode.ClampToEdge,
                AddressW = WrapMode.ClampToEdge,
                MagFilter = FilterMode.Linear,
                MinFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                MinLOD = 0f,
                MaxLOD = float.MaxValue,
                MaxAnisotropy = 1
            });
        }

        public Texture Process(PostProcessContext ctx, Texture input, int mipCount, float filterRadius, MipMode mipMode, string namePrefix, bool usePrefilter = true)
        {
            BuildMipChain(ctx, input, mipCount, mipMode, namePrefix);

            if (usePrefilter)
                RenderPrefilter(ctx, input);

            RenderDownsamples(ctx, filterRadius);

            RenderUpsamples(ctx, filterRadius, namePrefix);

            return upsampleMipList[0].Texture;
        }

        private void BeginPass(PostProcessContext ctx, Texture texture, Vector2 size)
        {
            ctx.Renderer.PushViewport(
                ctx.CommandList,
                new ViewportRect
                {
                    Width = (int)size.X,
                    Height = (int)size.Y
                });

            target.SetColorAttachment(0, texture);

            ctx.CommandList.SetFramebuffer(
                target.GPU);

            ctx.CommandList.ClearColor(0, Colors.Transparent);
        }

        private void EndPass(PostProcessContext ctx, bool restoreViewport = true)
        {
            ctx.Renderer.PopViewport(
                ctx.CommandList,
                restoreViewport);
        }

        private void BindMipData(MaterialInstance material, Vector2 inputSize, float filterRadius, bool upsample)
        {
            mipShaderDataBuffer.Update(
                new BloomMipShaderData
                {
                    mipSize = inputSize,
                    filterRadius = filterRadius
                });

            material.DescriptorSet.SetSampler(
                0,
                upsample
                    ? upsampleBloomSampler.GPU
                    : downsampleBloomSampler.GPU);

            material.DescriptorSet.SetUniformBuffer(
                5,
                mipShaderDataBuffer.GPU);
        }

        private void RenderPrefilter(PostProcessContext ctx, Texture input)
        {
            BloomMip firstMip = bloomMipList[0];

            BeginPass(
                ctx,
                firstMip.Texture,
                firstMip.Size);

            prefilterMaterial.SetTexture(
                "INPUT_TEXTURE",
                input);

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                prefilterMaterial);

            EndPass(ctx);
        }

        private void RenderDownsamples(PostProcessContext ctx, float filterRadius)
        {
            for (int i = 1; i < bloomMipList.Count; i++)
            {
                BloomMip mip = bloomMipList[i];
                BloomMip previousMip = bloomMipList[i - 1];

                BeginPass(
                    ctx,
                    mip.Texture,
                    mip.Size);

                downsampleMaterial.SetTexture(
                    "INPUT_TEXTURE",
                    previousMip.Texture);

                BindMipData(
                    downsampleMaterial,
                    mip.Size,
                    filterRadius,
                    false);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    downsampleMaterial);

                EndPass(
                    ctx,
                    i == bloomMipList.Count - 1);
            }
        }

        private void RenderUpsamples(
            PostProcessContext ctx,
            float filterRadius,
            string namePrefix)
        {
            upsampleMipList.Clear();
            upsampleMipList.AddRange(bloomMipList);

            for (int i = upsampleMipList.Count - 2; i >= 0; i--)
            {
                BloomMip currentMip = upsampleMipList[i];
                BloomMip previousMip = upsampleMipList[i + 1];

                TextureDescription desc =
                    currentMip.Texture.GPU.Description;

                desc.MipLevels = 1;

                Texture output =
                    ctx.RenderContext.Resources.GetOrCreateTexture(
                        $"{namePrefix}_UP_{i}",
                        desc);

                BeginPass(
                    ctx,
                    output,
                    currentMip.Size);

                upsampleMaterial.SetTexture(
                    "INPUT_TEXTURE",
                    currentMip.Texture);

                upsampleMaterial.SetTexture(
                    "PREVIOUS_TEXTURE",
                    previousMip.Texture);

                BindMipData(
                    upsampleMaterial,
                    previousMip.Size,
                    filterRadius,
                    true);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    upsampleMaterial);

                EndPass(
                    ctx,
                    i == 0);

                upsampleMipList[i] =
                    new BloomMip(
                        currentMip.Size,
                        output);
            }
        }

        private void BuildMipChain(
            PostProcessContext ctx,
            Texture input,
            int mipCount,
            MipMode mipMode,
            string namePrefix)
        {
            bloomMipList.Clear();

            TextureDescription desc =
                input.GPU.Description;

            int width = Math.Max(
                desc.Width >> 1,
                1);

            int height = Math.Max(
                desc.Height >> 1,
                1);

            for (int i = 0; i < mipCount; i++)
            {
                desc.Width = width;
                desc.Height = height;
                desc.MipLevels = 1;

                Texture texture =
                    ctx.RenderContext.Resources.GetOrCreateTexture(
                        $"{namePrefix}_{i}",
                        desc);

                bloomMipList.Add(
                    new BloomMip(
                        new Vector2(width, height),
                        texture));

                if (width == 1 && height == 1)
                    break;

                switch (mipMode)
                {
                    case MipMode.Normal:
                        width = Math.Max(
                            width >> 1,
                            1);

                        height = Math.Max(
                            height >> 1,
                            1);
                        break;

                    case MipMode.Vertical:
                        height = Math.Max(
                            height >> 1,
                            1);
                        break;

                    case MipMode.Horizontal:
                        width = Math.Max(
                            width >> 1,
                            1);
                        break;
                }
            }
        }
    }
}