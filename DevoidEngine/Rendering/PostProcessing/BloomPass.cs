using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidEngine.Rendering.PostProcessing;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Rendering.PostProcessing
{
    public class BloomPass : PostProcessPass
    {
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
        public int BloomMipCount { get; set; } = 8;
        public float BloomRadius { get; set; } = 1;

        private readonly MaterialInstance prefilterMaterial;
        private readonly MaterialInstance downsampleMaterial;
        private readonly MaterialInstance upsampleMaterial;

        private readonly RenderTarget target;
        private readonly UniformBuffer mipShaderDataBuffer;
        private readonly Sampler downsampleBloomSampler;
        private readonly Sampler upsampleBloomSampler;

        private readonly List<BloomMip> bloomMipList = [];
        private readonly List<string> bloomMipNames = [];

        private readonly List<BloomMip> upsampleMipList = [];
        private readonly List<string> upsampleMipNames = [];

        public BloomPass()
        {
            prefilterMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_process_pass.dsd"))));
            downsampleMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_downsample_pass.dsd"))));
            upsampleMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_upsample_pass.dsd"))));

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

            for (int i = 0; i < BloomMipCount; i++)
            {
                bloomMipNames.Add($"PP_BLOOM_{i}");
                upsampleMipNames.Add($"PP_BLOOM_UP_{i}");
            }
        }
        public override void Setup()
        {
            Read("SceneColor");
            Write("Bloom");
        }

        public override void Execute(PostProcessContext ctx)
        {
            Texture scene = ctx.GetTexture("SceneColor");

            BuildMipChain(ctx, scene);

            RenderPrefilter(ctx);
            RenderDownsamples(ctx);
            RenderUpsamples(ctx);

            ctx.SetTexture("Bloom", upsampleMipList[0].Texture);
        }

        private void BeginPass(PostProcessContext ctx, Texture texture, Vector2 size)
        {
            ctx.Renderer.PushViewport(ctx.CommandList, new ViewportRect
            {
                Width = (int)size.X,
                Height = (int)size.Y
            });

            target.SetColorAttachment(0, texture);
            ctx.CommandList.SetFramebuffer(target.GPU);
            ctx.CommandList.ClearColor(0, Colors.Transparent);
        }

        private void EndPass(PostProcessContext ctx, bool restoreViewport = true)
        {
            ctx.Renderer.PopViewport(ctx.CommandList, restoreViewport);
        }

        private void BindMipData(MaterialInstance material, Vector2 inputSize, bool upsample = false)
        {
            mipShaderDataBuffer.Update(new BloomMipShaderData
            {
                mipSize = inputSize,
                filterRadius = BloomRadius
            });

            material.DescriptorSet.SetSampler(
                0,
                upsample
                    ? upsampleBloomSampler.GPU
                    : downsampleBloomSampler.GPU);

            material.DescriptorSet.SetUniformBuffer(
                2,
                mipShaderDataBuffer.GPU);
        }

        private void RenderPrefilter(PostProcessContext ctx)
        {
            Texture sceneColor = ctx.GetTexture("SceneColor");

            BloomMip firstMip = bloomMipList[0];

            BeginPass(ctx, firstMip.Texture, firstMip.Size);

            prefilterMaterial.SetTexture("INPUT_TEXTURE", sceneColor);

            ctx.Renderer.API.RenderToScreen(ctx.CommandList, prefilterMaterial);

            EndPass(ctx);
        }

        private void RenderDownsamples(PostProcessContext ctx)
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

                BindMipData(downsampleMaterial, mip.Size);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    downsampleMaterial);

                EndPass(
                    ctx,
                    i == bloomMipList.Count - 1);
            }
        }

        private void RenderUpsamples(PostProcessContext ctx)
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
                        upsampleMipNames[i],
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
        private void BuildMipChain(PostProcessContext ctx, Texture scene)
        {
            bloomMipList.Clear();

            TextureDescription desc = scene.GPU.Description;

            int width = Math.Max(desc.Width >> 1, 1);
            int height = Math.Max(desc.Height >> 1, 1);

            for (int i = 0; i < BloomMipCount; i++)
            {
                desc.Width = width;
                desc.Height = height;
                desc.MipLevels = 1;

                Texture texture =
                    ctx.RenderContext.Resources.GetOrCreateTexture(
                        bloomMipNames[i],
                        desc);

                bloomMipList.Add(new BloomMip(
                    new Vector2(width, height),
                    texture));

                if (width == 1 && height == 1)
                    break;

                width = Math.Max(width >> 1, 1);
                height = Math.Max(height >> 1, 1);
            }
        }

    }
}