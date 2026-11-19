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
        private readonly MaterialInstance _material;
        private readonly Sampler _textureSampler;

        private readonly RenderTarget _target;
        private readonly PostProcessSettings _state;

        public TonemapPass()
        {
            _material = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/tonemap_pass.dsd"))));

            _target = RenderTarget.Create(1);
            _state = new()
            {
                AnamorphicBloomEnabled = false,
                BloomEnabled = false,
                BloomIntensity = 0,
                AnamorphicBloomIntensity = 0,
                Exposure = 0
            };


            _textureSampler = Sampler.Create(new SamplerDescription
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

            _material.SetSampler("TonemapSampler", _textureSampler);
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

            TextureDescription desc = scene!.Gpu.Description;

            Texture output = ctx.RenderContext.Resources.GetOrCreateTexture("PP_TONEMAP", desc);

            _target.SetColorAttachment(0, output);

            _material.SetTexture("MAT_SceneColor", scene);
            if (bloom != null)
                _material.SetTexture("MAT_BloomColor", bloom);
            if (anamorphicBloom != null)
                _material.SetTexture("MAT_AnamorphicBloomColor", anamorphicBloom);

            ctx.CommandList.SetFramebuffer(_target.Gpu);

            ctx.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                _material);

            ctx.SetTexture("ToneMapped", output);
        }

        private void UpdateMaterialState(PostProcessSettings settings)
        {
            if (_state.BloomEnabled != settings.BloomEnabled)
            {
                _material.SetInt("bloomEnabled", settings.BloomEnabled ? 1 : 0);
                _state.BloomEnabled = settings.BloomEnabled;
            }

            if (_state.AnamorphicBloomEnabled != settings.AnamorphicBloomEnabled)
            {
                _material.SetInt("anamorphicBloomEnabled", settings.AnamorphicBloomEnabled ? 1 : 0);
                _state.AnamorphicBloomEnabled = settings.AnamorphicBloomEnabled;
            }

            if (_state.Exposure != settings.Exposure)
            {
                _material.SetFloat("exposure", settings.Exposure);
                _state.Exposure = settings.Exposure;
            }

            if (_state.BloomIntensity != settings.BloomIntensity)
            {
                _material.SetFloat("bloomIntensity", settings.BloomIntensity);
                _state.BloomIntensity = settings.BloomIntensity;
            }

            if (_state.AnamorphicBloomIntensity != settings.AnamorphicBloomIntensity)
            {
                _material.SetFloat("anamorphicIntensity", settings.AnamorphicBloomIntensity);
                _state.BloomIntensity = settings.AnamorphicBloomIntensity;
            }
        }
    }
}
