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

        private readonly RenderTarget target;

        public TonemapPass()
        {
            material = new MaterialInstance(
                new Material(
                    Shader.FromDescriptorFile(
                        Engine.GraphicsDevice,
                        Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/tonemap_pass.dsd"))));

            material.SetFloat("exposure", 0.6f);
            material.SetFloat("bloomIntensity", 1f);

            target = RenderTarget.Create(1);
        }

        public override void Setup()
        {
            Read("SceneColor");
            Read("Bloom");

            Write("ToneMapped");
        }

        public override void Execute(PostProcessContext ctx)
        {
            Texture scene = ctx.GetTexture("SceneColor");
            Texture bloom = ctx.GetTexture("Bloom");

            TextureDescription desc = scene.GPU.Description;

            Texture output = ctx.RenderContext.Resources.GetOrCreateTexture(
                "PP_TONEMAP",
                desc);

            target.SetColorAttachment(0, output);

            material.SetTexture("MAT_SceneColor", scene);
            material.SetTexture("MAT_BloomColor", bloom);
            ctx.CommandList.SetFramebuffer(target.GPU);

            ctx.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));

            ctx.Renderer.API.RenderToScreen(
                ctx.CommandList,
                material);

            ctx.SetTexture("ToneMapped", output);
        }

        public override void Resize(int width, int height)
        {
        }
    }
}
