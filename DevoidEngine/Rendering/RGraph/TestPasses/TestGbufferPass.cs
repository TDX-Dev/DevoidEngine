using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph.TestPasses
{
    internal class TestGbufferPass : RenderPass
    {
        public TestGbufferPass()
        {
            Name = "GBuffer Pass";
        }

        public override void Setup(RenderGraphContext ctx, RenderGraphBuilder builder)
        {
            TextureDescription gbufferOutputDesc = new()
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

            RID gbufferHandle = builder.CreateTexture("GBufferOutput", gbufferOutputDesc);

            builder.AddTextureDependency(gbufferHandle, RGResourceAccessType.Write);
        }
    }
}
