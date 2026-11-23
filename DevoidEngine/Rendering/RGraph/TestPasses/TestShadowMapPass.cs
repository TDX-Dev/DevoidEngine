using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph.TestPasses
{
    internal class TestShadowMapPass : RenderPass
    {
        public TestShadowMapPass()
        {
            Name = "ShadowMapPass";
        }

        public override void Setup(RenderGraphBuilder builder)
        {
            TextureDescription shadowMapOutputDesc = new()
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

            RID shadowMapHandle = builder.CreateTexture("ShadowMapOutput", shadowMapOutputDesc);

            builder.AddTextureDependency(shadowMapHandle, RGResourceAccessType.Write);
        }


    }
}
