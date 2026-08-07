using DevoidEngine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public class BloomPass : PostProcessPass
    {
        struct BloomMip
        {
            public Vector2 Size;
            public Texture Texture;
        }

        struct BloomMipShaderData
        {
            public Vector2 mipSize;
            public int mipLevel;
            public float filterRadius;
        }

        public int BloomMipCount { get; set; } = 7;
        public float BloomFilterRadius { get; set; }

        private readonly MaterialInstance processMaterial;
        private readonly MaterialInstance downsampleMaterial;
        private readonly MaterialInstance upsampleMaterial;

        private readonly RenderTarget target;

        private readonly UniformBuffer mipShaderDataBuffer;

        private readonly Sampler BloomSampler;
        private Texture prefilterTexture = null!;

        private readonly List<BloomMip> bloomMipList = [];

        public BloomPass()
        {
            processMaterial = new MaterialInstance(
                new Material(
                    Shader.FromDescriptorFile(
                        Engine.GraphicsDevice,
                        Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_process_pass.dsd"))));

            downsampleMaterial = new MaterialInstance(
                new Material(
                    Shader.FromDescriptorFile(
                        Engine.GraphicsDevice,
                        Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_downsample_pass.dsd"))));

            upsampleMaterial = new MaterialInstance(
                new Material(
                    Shader.FromDescriptorFile(
                        Engine.GraphicsDevice,
                        Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/bloom_upsample_pass.dsd"))));

            target = RenderTarget.Create(1);

            mipShaderDataBuffer = new UniformBuffer(Engine.GraphicsDevice, ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<BloomMipShaderData>());

            BloomSampler = Sampler.Create(new SamplerDescription()
            {
                AddressU = WrapMode.ClampToEdge,
                AddressV = WrapMode.ClampToEdge,
                AddressW = WrapMode.ClampToEdge,
                MagFilter = FilterMode.Linear,
                MinFilter = FilterMode.Linear,
                MipFilter = FilterMode.Linear,
                MinLOD = 0f,
                MaxLOD = float.MaxValue,
                MaxAnisotropy = 1,
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

            RenderProcess(ctx);

            RenderDownsamples(ctx);

            RenderUpsamples(ctx);

            ctx.SetTexture("Bloom", bloomMipList[0].Texture);
        }

        private void RenderProcess(PostProcessContext ctx)
        {
            Texture sceneColor = ctx.GetTexture("SceneColor");

            prefilterTexture = ctx.RenderContext.Resources.GetOrCreateTexture($"PP_BLOOM_PREFILTER", sceneColor.GPU.Description);

            target.SetColorAttachment(0, prefilterTexture);
            ctx.CommandList.SetFramebuffer(target.GPU);


            processMaterial.SetTexture("INPUT_TEXTURE", sceneColor);

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                processMaterial
            );
        }

        private void RenderDownsamples(PostProcessContext ctx)
        {
            for (int i = 0; i < bloomMipList.Count; i++)
            {
                BloomMip mip = bloomMipList[i];

                ctx.Renderer.PushViewport(ctx.CommandList, new ViewportRect()
                {
                    X = 0,
                    Y = 0,
                    Width = (int)mip.Size.X,
                    Height = (int)mip.Size.Y
                });

                target.SetColorAttachment(0, mip.Texture);

                ctx.CommandList.SetFramebuffer(target.GPU);
                //ctx.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));

                Texture source;
                Vector2 sourceSize;

                if (i == 0)
                {
                    source = prefilterTexture;// ctx.GetTexture("SceneColor");

                    TextureDescription desc = source.GPU.Description;

                    sourceSize = new Vector2(desc.Width, desc.Height);
                }
                else
                {
                    source = bloomMipList[i - 1].Texture;
                    sourceSize = bloomMipList[i - 1].Size;
                }

                downsampleMaterial.SetTexture(
                    "INPUT_TEXTURE",
                    source);

                mipShaderDataBuffer.Update(new BloomMipShaderData
                {
                    mipLevel = i,
                    mipSize = sourceSize,
                    filterRadius = BloomFilterRadius
                });

                downsampleMaterial.DescriptorSet.SetSampler(0, BloomSampler.GPU);
                downsampleMaterial.DescriptorSet.SetUniformBuffer(2, mipShaderDataBuffer.GPU);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    downsampleMaterial);

                ctx.Renderer.PopViewport(ctx.CommandList);
            }
        }

        private void RenderUpsamples(PostProcessContext ctx)
        {
            for (int i = bloomMipList.Count - 1; i > 0; i--)
            {
                BloomMip sourceMip = bloomMipList[i];
                BloomMip destMip = bloomMipList[i - 1];

                ctx.Renderer.PushViewport(ctx.CommandList, new ViewportRect()
                {
                    X = 0,
                    Y = 0,
                    Width = (int)destMip.Size.X,
                    Height = (int)destMip.Size.Y
                });

                target.SetColorAttachment(0, destMip.Texture);

                ctx.CommandList.SetFramebuffer(target.GPU);
                //ctx.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));

                //ctx.CommandList.SetBlendMode(BlendMode.Additive);

                upsampleMaterial.SetTexture(
                    "INPUT_TEXTURE",
                    sourceMip.Texture);

                mipShaderDataBuffer.Update(new BloomMipShaderData
                {
                    mipLevel = i,
                    mipSize = sourceMip.Size,
                    filterRadius = BloomFilterRadius
                });

                upsampleMaterial.DescriptorSet.SetSampler(0, BloomSampler.GPU);
                upsampleMaterial.DescriptorSet.SetUniformBuffer(2, mipShaderDataBuffer.GPU);

                ctx.Renderer.API.RenderToScreen(
                    ctx.CommandList,
                    upsampleMaterial);

                ctx.Renderer.PopViewport(ctx.CommandList);
            }

            //ctx.CommandList.SetBlendMode(BlendMode.Opaque);
        }

        private void BuildMipChain(PostProcessContext ctx, Texture scene)
        {
            bloomMipList.Clear();

            TextureDescription desc = scene.GPU.Description;

            int width = desc.Width;
            int height = desc.Height;

            for (int i = 0; i < BloomMipCount; i++)
            {
                width = Math.Max(1, width / 2);
                height = Math.Max(1, height / 2);

                TextureDescription mipDesc = desc;

                mipDesc.Width = width;
                mipDesc.Height = height;
                mipDesc.MipLevels = 1;

                Texture texture =
                    ctx.RenderContext.Resources.GetOrCreateTexture(
                        $"PP_BLOOM_{i}",
                        mipDesc);

                bloomMipList.Add(new BloomMip
                {
                    Size = new Vector2(width, height),
                    Texture = texture
                });

                if (width == 1 || height == 1)
                    break;
            }
        }
    }
}
