using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidEngine.Rendering.PostProcessing;
using DevoidGPU;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        public int mipLevel;
        public float filterRadius;
    }
    public int BloomMipCount { get; set; } = 7;
    public float BloomFilterRadius { get; set; }

    private readonly MaterialInstance prefilterMaterial;
    private readonly MaterialInstance downsampleMaterial;
    private readonly MaterialInstance upsampleMaterial;

    private readonly RenderTarget target;
    private readonly UniformBuffer mipShaderDataBuffer;
    private readonly Sampler bloomSampler;

    private readonly List<BloomMip> bloomMipList = [];

    private Texture prefilterTexture = null!;

    public BloomPass()
    {
        MaterialInstance LoadMaterial(string descriptor)
        {
            return new MaterialInstance(
                new Material(
                    Shader.FromDescriptorFile(
                        Engine.GraphicsDevice,
                        Path.Combine(
                            Engine.BasePath,
                            $"Content/DevoidShaderDescriptors/{descriptor}"))));
        }

        prefilterMaterial = LoadMaterial("bloom_process_pass.dsd");
        downsampleMaterial = LoadMaterial("bloom_downsample_pass.dsd");
        upsampleMaterial = LoadMaterial("bloom_upsample_pass.dsd");

        target = RenderTarget.Create(1);

        mipShaderDataBuffer = new UniformBuffer(
            Engine.GraphicsDevice,
            ResourceUsage.Dynamic,
            (uint)Unsafe.SizeOf<BloomMipShaderData>());

        bloomSampler = Sampler.Create(new SamplerDescription
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

        ctx.SetTexture("Bloom", bloomMipList[0].Texture);
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
    }

    private void EndPass(PostProcessContext ctx, bool restoreViewport = true)
    {
        ctx.Renderer.PopViewport(ctx.CommandList, restoreViewport);
    }

    private void BindMipData(
    MaterialInstance material,
    int mipLevel,
    Vector2 sourceSize)
    {
        mipShaderDataBuffer.Update(new BloomMipShaderData
        {
            mipLevel = mipLevel,
            mipSize = sourceSize,
            filterRadius = BloomFilterRadius
        });

        material.DescriptorSet.SetSampler(0, bloomSampler.GPU);
        material.DescriptorSet.SetUniformBuffer(2, mipShaderDataBuffer.GPU);
    }

    private void RenderPrefilter(PostProcessContext ctx)
    {
        Texture sceneColor = ctx.GetTexture("SceneColor");

        prefilterTexture = ctx.RenderContext.Resources.GetOrCreateTexture(
            "PP_BLOOM_PREFILTER",
            sceneColor.GPU.Description);

        target.SetColorAttachment(0, prefilterTexture);
        ctx.CommandList.SetFramebuffer(target.GPU);

        prefilterMaterial.SetTexture("INPUT_TEXTURE", sceneColor);

        ctx.Renderer.API.RenderToScreen(
            ctx.CommandList,
            prefilterMaterial);
    }

    private void RenderDownsamples(PostProcessContext ctx)
    {
        for (int i = 0; i < bloomMipList.Count; i++)
        {
            BloomMip mip = bloomMipList[i];

            BeginPass(ctx, mip.Texture, mip.Size);

            Texture source;
            Vector2 sourceSize;

            if (i == 0)
            {
                source = prefilterTexture;

                TextureDescription desc = source.GPU.Description;
                sourceSize = new Vector2(desc.Width, desc.Height);
            }
            else
            {
                BloomMip previousMip = bloomMipList[i - 1];

                source = previousMip.Texture;
                sourceSize = previousMip.Size;
            }

            downsampleMaterial.SetTexture("INPUT_TEXTURE", source);
            BindMipData(downsampleMaterial, i, sourceSize);

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                downsampleMaterial);

            EndPass(ctx, i == (bloomMipList.Count - 1));
        }
    }

    private void RenderUpsamples(PostProcessContext ctx)
    {
        for (int i = bloomMipList.Count - 1; i > 0; i--)
        {
            BloomMip sourceMip = bloomMipList[i];
            BloomMip destinationMip = bloomMipList[i - 1];

            BeginPass(ctx, destinationMip.Texture, destinationMip.Size);

            upsampleMaterial.SetTexture(
                "INPUT_TEXTURE",
                sourceMip.Texture);

            BindMipData(
                upsampleMaterial,
                i,
                sourceMip.Size);

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                upsampleMaterial);

            EndPass(ctx, i == 1);
        }
    }
    private void BuildMipChain(PostProcessContext ctx, Texture scene)
    {
        bloomMipList.Clear();

        TextureDescription desc = scene.GPU.Description;

        int width = desc.Width;
        int height = desc.Height;

        for (int i = 0; i < BloomMipCount; i++)
        {
            width = Math.Max(width >> 1, 1);
            height = Math.Max(height >> 1, 1);

            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;

            Texture texture = ctx.RenderContext.Resources.GetOrCreateTexture(
                $"PP_BLOOM_{i}",
                desc);

            bloomMipList.Add(new BloomMip(
                new Vector2(width, height),
                texture));

            if (width == 1 && height == 1)
                break;
        }
    }

}