using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.DebugTools;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering.PostProcessing
{
    public class VolumetricLightPass : RenderGraphPass
    {

        Texture2D output;
        Framebuffer framebuffer;

        Shader volumetricShader;
        MaterialInstance material;
        public override Texture2D OutputTexture => output;


        public VolumetricLightPass(int width, int height)
        {
            volumetricShader = new Shader("Engine/Content/Shaders/Volumetric/volumetric");
            material = new MaterialInstance(new Material(volumetricShader));


            output = new Texture2D(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA16_Float,
                IsRenderTarget = true
            });

            framebuffer = new Framebuffer();
            framebuffer.AttachRenderTexture(output);
        }



        public override void Setup()
        {
            Write("VolumetricOutput");
        }

        public override void Execute(RenderGraphContext ctx)
        {
            framebuffer.Bind();
            framebuffer.Clear(new Vector4(0));

            Renderer.GraphicsDevice.SetBlendState(BlendMode.Additive);

            material.SetTexture("depthTexture", ctx.GetTexture("Depth"));
            material.SetTexture("shadowMap", ctx.GetTexture("ShadowMap"));

            RenderAPI.RenderFullScreen(material);

            Renderer.GraphicsDevice.SetBlendState(BlendMode.Opaque);

            ctx.SetTexture("VolumetricOutput", output);
        }

        public override void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
        }
    }
}