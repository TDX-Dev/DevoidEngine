using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph.TestPasses
{
    internal class TestBloomPass : RenderPass
    {
        public TestBloomPass()
        {
            Name = "Bloom Pass";
        }

        public override void Setup(RenderGraphBuilder builder)
        {
            TextureDescription bloomPassOutput = new()
            {
                Height = 64,
                Width = 64,
                Depth = 1,
                Dimension = TextureDimension.Texture2D,
                MipLevels = 1,
                Samples = new TextureSampleDescription(1, 0),
                Usage = TextureUsage.ShaderResource,
                ArraySize = 1,
                Format = TextureFormat.RGBA16Float
            };

            RID bloomPassTextureHandle = builder.CreateTexture("BloomOutput", bloomPassOutput);
            RID lightingPassTextureHandle = builder.FindTexture("LightingPassOutput")!.Value;

            builder.AddTextureDependency(lightingPassTextureHandle, RGResourceAccessType.Read);
            builder.AddTextureDependency(bloomPassTextureHandle, RGResourceAccessType.Write);
        }
    }
}
