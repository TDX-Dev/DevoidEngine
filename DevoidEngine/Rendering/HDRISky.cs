using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public sealed class HDRISky : ISky
    {
        public MaterialInstance Material => throw new NotImplementedException();

        public Mesh Mesh => throw new NotImplementedException();

        public bool Dirty { get; set; } = true;

        public Texture? PanoramaTexture;

        public void BuildEnvironment(RenderContext context, EnvironmentLighting environment)
        {
            if (PanoramaTexture == null)
                return;

            Engine.Renderer.SkyRenderer.ConvertPanoramaToCubemap(context.CommandList, PanoramaTexture);


            Dirty = false;
        }

        public void Dispose()
        {

        }
    }
}
