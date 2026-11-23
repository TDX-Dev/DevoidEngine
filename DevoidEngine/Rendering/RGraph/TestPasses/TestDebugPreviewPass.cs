using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph.TestPasses
{
    public class TestDebugPreviewPass : RenderPass
    {
        public TestDebugPreviewPass()
        {
            Name = "DebugPreviewPass";
        }
        public override void Setup(RenderGraphBuilder builder)
        {
            TextureDescription debugOutputDesc = new()
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

            RID debugTexture = builder.CreateTexture("DebugTexture", debugOutputDesc);

            builder.AddTextureDependency(debugTexture, RGResourceAccessType.Write);
        }

    }
}
