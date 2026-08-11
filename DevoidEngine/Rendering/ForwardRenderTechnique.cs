using DevoidEngine.Core;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public sealed class ForwardRenderTechnique : IRenderTechnique
    {

        private TextureDescription lightingColorTextureDescription;
        private TextureDescription lightingDepthTextureDescription;
        private TextureDescription lightingColorResolveTextureDescription;

        private RenderTarget colorOutput = null!;
        private List<RenderMeshData> visibleItems = null!;


        public void Initialize()
        {
            visibleItems = [];
            colorOutput = RenderTarget.Create(1);

            //colorOutput = RenderTarget.Create();
        }

        public RenderTarget Render(RenderContext ctx, RenderView view)
        {
            Viewport viewport = ctx.Viewport;
            lightingColorTextureDescription = new TextureDescription()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                Depth = 1,
                Format = TextureFormat.RGBA16_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(4, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };
            lightingDepthTextureDescription = new TextureDescription()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                Depth = 1,
                Format = TextureFormat.Depth24_Stencil8,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(4, 0),
                MipLevels = 1,
                Usage = TextureUsage.DepthStencil
            };

            lightingColorResolveTextureDescription = new TextureDescription()
            {
                Width = viewport.Width,
                Height = viewport.Height,
                Depth = 1,
                Format = TextureFormat.RGBA16_Float,
                Dimension = TextureDimension.Texture2D,
                ArraySize = 1,
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.RenderTarget | TextureUsage.ShaderResource
            };

            Texture lightingColorTex = ctx.Resources.GetOrCreateTexture("ForwardLightingColor", lightingColorTextureDescription);
            Texture lightingDepthTex = ctx.Resources.GetOrCreateTexture("ForwardLightingDepth", lightingDepthTextureDescription);

            Texture lightingResolvedColorTex = ctx.Resources.GetOrCreateTexture("ForwardLightingColorResolved", lightingColorResolveTextureDescription);

            colorOutput.SetColorAttachment(0, lightingColorTex);
            colorOutput.SetDepthAttachment(lightingDepthTex);

            ctx.CommandList.SetFramebuffer(colorOutput.GPU);

            ctx.CommandList.ClearColor(0, new Vector4(0, 0, 0, 1));
            ctx.CommandList.ClearDepthStencil(1, 0);

            ctx.Renderer.SkyRenderer.RenderSkybox(ctx);
            ctx.Renderer.Execute(ctx.CommandList, view.Objects);

            ctx.CommandList.ResolveSubresource(lightingColorTex.GPU, lightingResolvedColorTex.GPU);

            colorOutput.SetColorAttachment(0, lightingResolvedColorTex);

            return colorOutput;
        }

        public void Dispose()
        {

        }
    }
}
