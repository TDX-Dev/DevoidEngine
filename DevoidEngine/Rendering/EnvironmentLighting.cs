using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public sealed class EnvironmentLighting : IDisposable
    {
        // Background
        public Texture SkyCubemap = null!;

        // Diffuse IBL
        public Texture IrradianceMap = null!;

        // Specular IBL
        public Texture PrefilterMap = null!;

        // BRDF Integration
        public Texture BRDFLUT = null!;

        public void Dispose()
        {
            SkyCubemap?.Dispose();
            IrradianceMap?.Dispose();
            PrefilterMap?.Dispose();
        }
    }
}
