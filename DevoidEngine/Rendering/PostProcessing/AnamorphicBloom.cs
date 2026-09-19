using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidGPU;

namespace DevoidEngine.Rendering.PostProcessing
{
    public class AnamorphicBloomPass : PostProcessPass
    {
        public int BloomMipCount { get; set; } = 6;
        public float BloomRadius { get; set; } = 0.7f;

        private readonly BloomUtility bloomUtility;

        public AnamorphicBloomPass()
        {
            bloomUtility = new BloomUtility();
        }

        public override void Setup()
        {
            Read("SceneColor");
            Write("AnamorphicBloom");
        }

        public override void Execute(PostProcessContext ctx)
        {
            Texture scene = ctx.GetTexture("SceneColor");

            Texture bloom = bloomUtility.Process(ctx, scene, BloomMipCount, BloomRadius, BloomUtility.MipMode.Vertical, "PP_ANAMORPHICBLOOM");

            ctx.SetTexture("AnamorphicBloom", bloom);
        }
    }
}