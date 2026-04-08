using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.DebugTools;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering.PostProcessing
{
    public class VolumetricLightPass : RenderGraphPass
    {
        Texture2D output;
        Framebuffer framebuffer;

        MaterialInstance material;
        public override Texture2D OutputTexture => output;

        private float exposure = 0.7f;
        private float bloomIntensity = 0.35f;

        public VolumetricLightPass(int width, int height)
        {
            material = new MaterialInstance(new Material(new Shader("Engine/Content/Shaders/Testing/tonemap")));
            material.SetFloat("exposure", exposure);
            material.SetFloat("bloomIntensity", bloomIntensity);

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
            Read("SceneColor");
            Read("BloomOutput");
            Write("ToneMapped");
        }

        public override void Execute(RenderGraphContext ctx)
        {
            var input = ctx.GetTexture("SceneColor");
            var bloomInput = ctx.GetTexture("BloomOutput");

            material.SetTexture("MAT_SceneColor", input);
            material.SetTexture("MAT_BloomColor", bloomInput);
            RenderAPI.RenderToBuffer(material, framebuffer);
            //RenderAPI.RenderToBuffer(input, framebuffer);

            ctx.SetTexture("ToneMapped", output);
        }

        public override void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
        }
    }
}