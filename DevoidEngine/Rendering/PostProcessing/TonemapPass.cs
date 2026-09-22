using DevoidEngine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public sealed class TonemapPass : PostProcessPass
    {
        private readonly MaterialInstance material;
        private readonly Sampler textureSampler;

        private readonly RenderTarget target;
        private readonly PostProcessSettings state;

        public TonemapPass()
        {
            material = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/tonemap_pass.dsd"))));

            target = RenderTarget.Create(1);
            state = new()
            {
                AnamorphicBloomEnabled = false,
                BloomEnabled = false,
                BloomIntensity = 0,
                AnamorphicBloomIntensity = 0,
                Exposure = 0
            };


            textureSampler = Sampler.Create(new SamplerDescription
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

            material.SetSampler("TonemapSampler", textureSampler);
        }

        public override void Setup()
        {
            Read("SceneColor");
            Read("Bloom");
            Read("AnamorphicBloom");

            Write("ToneMapped");
        }

        public override void Execute(PostProcessContext ctx)
        {
            UpdateMaterialState(ctx.Settings);

            Texture? scene = ctx.GetTexture("SceneColor");
            Texture? bloom = ctx.GetTexture("Bloom");
            Texture? anamorphicBloom = ctx.GetTexture("AnamorphicBloom");

            TextureDescription desc = scene!.GPU.Description;

            Texture output = ctx.RenderContext.Resources.GetOrCreateTexture("PP_TONEMAP", desc);

            target.SetColorAttachment(0, output);

            material.SetTexture("MAT_SceneColor", scene);
            if (bloom != null)
                material.SetTexture("MAT_BloomColor", bloom);
            if (anamorphicBloom != null)
                material.SetTexture("MAT_AnamorphicBloomColor", anamorphicBloom);

            ctx.CommandList.SetFramebuffer(target.GPU);

            ctx.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                material);

            ctx.SetTexture("ToneMapped", output);
        }

        private void UpdateMaterialState(PostProcessSettings settings)
        {
            if (state.BloomEnabled != settings.BloomEnabled)
            {
                material.SetInt("bloomEnabled", settings.BloomEnabled ? 1 : 0);
                state.BloomEnabled = settings.BloomEnabled;
            }

            if (state.AnamorphicBloomEnabled != settings.AnamorphicBloomEnabled)
            {
                material.SetInt("anamorphicBloomEnabled", settings.AnamorphicBloomEnabled ? 1 : 0);
                state.AnamorphicBloomEnabled = settings.AnamorphicBloomEnabled;
            }

            if (state.Exposure != settings.Exposure)
            {
                material.SetFloat("exposure", settings.Exposure);
                state.Exposure = settings.Exposure;
            }

            if (state.BloomIntensity != settings.BloomIntensity)
            {
                material.SetFloat("bloomIntensity", settings.BloomIntensity);
                state.BloomIntensity = settings.BloomIntensity;
            }

            if (state.AnamorphicBloomIntensity != settings.AnamorphicBloomIntensity)
            {
                material.SetFloat("anamorphicIntensity", settings.AnamorphicBloomIntensity);
                state.BloomIntensity = settings.AnamorphicBloomIntensity;
            }
        }
    }
}
