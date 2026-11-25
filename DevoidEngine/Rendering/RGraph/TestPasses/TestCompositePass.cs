using DevoidEngine.Util;

namespace DevoidEngine.Rendering.RGraph.TestPasses
{
    internal class TestCompositePass : RenderPass
    {
        public TestCompositePass()
        {
            Name = "Composite Pass";
        }

        public override void Setup(RenderGraphContext ctx, RenderGraphBuilder builder)
        {

            RID compositePassTextureHandle = builder.FindTexture("Backbuffer")!.Value;
            RID lightingPassTextureHandle = builder.FindTexture("LightingPassOutput")!.Value;
            RID bloomPassTextureHandle = builder.FindTexture("BloomOutput")!.Value;

            builder.AddTextureDependency(lightingPassTextureHandle, RGResourceAccessType.Read);
            builder.AddTextureDependency(bloomPassTextureHandle, RGResourceAccessType.Read);
            builder.AddTextureDependency(compositePassTextureHandle, RGResourceAccessType.Write);
        }

    }
}
