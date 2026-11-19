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
            public Vector2 MipSize;
            public float FilterRadius;
        }

        private readonly MaterialInstance _prefilterMaterial;
        private readonly MaterialInstance _downsampleMaterial;
        private readonly MaterialInstance _upsampleMaterial;

        private readonly RenderTarget _target;
        private readonly UniformBuffer _mipShaderDataBuffer;
        private readonly Sampler _downsampleBloomSampler;
        private readonly Sampler _upsampleBloomSampler;

        private readonly List<BloomMip> _bloomMipList = [];
        private readonly List<BloomMip> _upsampleMipList = [];

        public BloomUtility()
        {
            this._prefilterMaterial = new(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_process_pass.dsd"))));
            this._downsampleMaterial = new(new(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_downsample_pass.dsd"))));
            this._upsampleMaterial = new(new(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_upsample_pass.dsd"))));

            _target = RenderTarget.Create(1);

            _mipShaderDataBuffer = new UniformBuffer(
                Engine.GraphicsDevice,
                ResourceUsage.Dynamic,
                (uint)Unsafe.SizeOf<BloomMipShaderData>());

            _downsampleBloomSampler = Sampler.Create(new SamplerDescription
            {
                AddressU = WrapMode.ClampToBorder,
                AddressV = WrapMode.ClampToBorder,
                AddressW = WrapMode.ClampToBorder,
                MagFilter = FilterMode.Linear,
                MinFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                MinLod = 0f,
                MaxLod = float.MaxValue,
                MaxAnisotropy = 1
            });

            _upsampleBloomSampler = Sampler.Create(new SamplerDescription
            {
                AddressU = WrapMode.ClampToEdge,
                AddressV = WrapMode.ClampToEdge,
                AddressW = WrapMode.ClampToEdge,
                MagFilter = FilterMode.Linear,
                MinFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                MinLod = 0f,
                MaxLod = float.MaxValue,
                MaxAnisotropy = 1
            });
        }

        public Texture Process(PostProcessContext ctx, Texture input, int mipCount, float filterRadius, MipMode mipMode, string namePrefix, bool usePrefilter = true, float bloomThreshold = 1.0f, float bloomKnee = 0.5f)
        {
            BuildMipChain(ctx, input, mipCount, mipMode, namePrefix);

            if (usePrefilter)
                RenderPrefilter(ctx, input, bloomThreshold, bloomKnee);

            RenderDownsamples(ctx, filterRadius);

            RenderUpsamples(ctx, filterRadius, namePrefix);

            return _upsampleMipList[0].Texture;
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

            _target.SetColorAttachment(0, texture);

            ctx.CommandList.SetFramebuffer(
                _target.Gpu);

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
            _mipShaderDataBuffer.Update(
                new BloomMipShaderData
                {
                    MipSize = inputSize,
                    FilterRadius = filterRadius
                });

            material.DescriptorSet.SetSampler(
                0,
                upsample
                    ? _upsampleBloomSampler.Gpu
                    : _downsampleBloomSampler.Gpu);

            material.DescriptorSet.SetUniformBuffer(5, _mipShaderDataBuffer.Gpu);
        }

        private void RenderPrefilter(PostProcessContext ctx, Texture input, float threshold, float knee)
        {
            BloomMip firstMip = _bloomMipList[0];

            BeginPass(ctx, firstMip.Texture, firstMip.Size);

            _prefilterMaterial.SetTexture("INPUT_TEXTURE", input);
            _prefilterMaterial.SetFloat("threshold", threshold);
            _prefilterMaterial.SetFloat("knee", knee);

            ctx.Renderer.API.RenderToScreen(ctx.CommandList, _prefilterMaterial);

            EndPass(ctx);
        }

        private void RenderDownsamples(PostProcessContext ctx, float filterRadius)
        {
            for (int i = 1; i < _bloomMipList.Count; i++)
            {
                BloomMip mip = _bloomMipList[i];
                BloomMip previousMip = _bloomMipList[i - 1];

                BeginPass(
                    ctx,
                    mip.Texture,
                    mip.Size);

                _downsampleMaterial.SetTexture(
                    "INPUT_TEXTURE",
                    previousMip.Texture);

                BindMipData(
                    _downsampleMaterial,
                    mip.Size,
                    filterRadius,
                    false);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    _downsampleMaterial);

                EndPass(
                    ctx,
                    i == _bloomMipList.Count - 1);
            }
        }

        private void RenderUpsamples(
            PostProcessContext ctx,
            float filterRadius,
            string namePrefix)
        {
            _upsampleMipList.Clear();
            _upsampleMipList.AddRange(_bloomMipList);

            for (int i = _upsampleMipList.Count - 2; i >= 0; i--)
            {
                BloomMip currentMip = _upsampleMipList[i];
                BloomMip previousMip = _upsampleMipList[i + 1];

                TextureDescription desc =
                    currentMip.Texture.Gpu.Description;

                desc.MipLevels = 1;

                Texture output =
                    ctx.RenderContext.Resources.GetOrCreateTexture(
                        $"{namePrefix}_UP_{i}",
                        desc);

                BeginPass(
                    ctx,
                    output,
                    currentMip.Size);

                _upsampleMaterial.SetTexture(
                    "INPUT_TEXTURE",
                    currentMip.Texture);

                _upsampleMaterial.SetTexture(
                    "PREVIOUS_TEXTURE",
                    previousMip.Texture);

                BindMipData(
                    _upsampleMaterial,
                    previousMip.Size,
                    filterRadius,
                    true);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    _upsampleMaterial);

                EndPass(
                    ctx,
                    i == 0);

                _upsampleMipList[i] =
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
            _bloomMipList.Clear();

            TextureDescription desc =
                input.Gpu.Description;

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

                _bloomMipList.Add(
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