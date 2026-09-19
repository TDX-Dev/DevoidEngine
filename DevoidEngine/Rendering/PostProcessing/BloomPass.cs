using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidGPU;

namespace DevoidEngine.Rendering.PostProcessing
{
    public class BloomPass : PostProcessPass
    {
        public int BloomMipCount { get; set; } = 8;
        public float BloomRadius { get; set; } = 0.7f;

        private readonly BloomUtility bloomUtility;

        public BloomPass()
        {
            bloomUtility = new BloomUtility();
        }

        public override void Setup()
        {
            Read("SceneColor");
            Write("Bloom");
        }

        public override void Execute(PostProcessContext ctx)
        {
            Texture scene = ctx.GetTexture("SceneColor");

            Texture bloom = bloomUtility.Process(ctx, scene, BloomMipCount, BloomRadius, BloomUtility.MipMode.Normal, "PP_BLOOM");

            ctx.SetTexture("Bloom", bloom);
        }
    }
}