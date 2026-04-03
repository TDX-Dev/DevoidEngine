using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.DebugTools;
using DevoidGPU;

namespace DevoidEngine.Engine.Rendering.PostProcessing
{
    public class TonemapPass : RenderGraphPass
    {
        Texture2D output;
        Framebuffer framebuffer;

        MaterialInstance material;

        public float Exposure
        {
            get => exposure;
            set
            {
                if (value == exposure)
                    return;

                exposure = value;
                material.SetFloat("exposure", value);
            }
        }

        public float BloomIntensity
        {
            get => bloomIntensity;
            set
            {
                if (value == bloomIntensity)
                    return;

                bloomIntensity = value;
                material.SetFloat("bloomIntensity", value);
            }
        }



        public override Texture2D OutputTexture => output;

        private float exposure = 0.7f;
        private float bloomIntensity = 0.35f;

        public TonemapPass(int width, int height)
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



            ConsoleRegistry.Instance.Register(
                new ConsoleVariable<float>(
                    "r.exposure",
                    () => Exposure,
                    e => Exposure = e,
                    "The Camera exposure in tonemap pass"
                )
            );
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