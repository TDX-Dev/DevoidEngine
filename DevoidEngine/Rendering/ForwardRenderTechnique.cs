using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public sealed class ForwardRenderTechnique : IRenderTechnique
    {

        private TextureDescription lightingColorTextureDescription;
        private TextureDescription lightingDepthTextureDescription;

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
                Samples =  new TextureSampleDescription(1, 0),
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
                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1,
                Usage = TextureUsage.DepthStencil
            };

            Texture lightingColorTex = ctx.Resources.GetOrCreateTexture("ForwardLightingColor", lightingColorTextureDescription);
            Texture lightingDepthTex = ctx.Resources.GetOrCreateTexture("ForwardLightingDepth", lightingDepthTextureDescription);

            colorOutput.SetColorAttachment(0, lightingColorTex);
            colorOutput.SetDepthAttachment(lightingDepthTex);

            ctx.CommandList.SetFramebuffer(colorOutput.GPU);

            ctx.CommandList.ClearColor(0, Vector4.Zero);
            ctx.CommandList.ClearDepthStencil(1, 0);

            ctx.Renderer.SkyRenderer.Render(ctx);
            ctx.Renderer.Execute(ctx.CommandList, view.Objects);

            return colorOutput;
        }

        public void Dispose()
        {

        }
    }
}
