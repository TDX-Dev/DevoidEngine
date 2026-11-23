using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph.TestPasses
{
    public class TestLightingPass : RenderPass
    {

        public TestLightingPass()
        {
            Name = "Lighting Pass";
        }

        public override void Setup(RenderGraphBuilder builder)
        {
            TextureDescription lightingPassOutput = new()
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

            RID lightingPassTextureHandle = builder.CreateTexture("LightingPassOutput", lightingPassOutput);
            RID shadowMapHandle = builder.FindTexture("ShadowMapOutput")!.Value;
            RID gbufferHandle = builder.FindTexture("GBufferOutput")!.Value;

            builder.AddTextureDependency(shadowMapHandle, RGResourceAccessType.Read);
            builder.AddTextureDependency(gbufferHandle, RGResourceAccessType.Read);
            builder.AddTextureDependency(lightingPassTextureHandle, RGResourceAccessType.Write);
        }
    }
}
