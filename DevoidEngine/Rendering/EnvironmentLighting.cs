using DevoidEngine.Core;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public sealed class EnvironmentLighting
    {
        public Texture Skybox = null!;
        public Texture Irradiance = null!;
        public Texture Prefilter = null!;
        public Texture BrdfLut = null!;

        public ShaderStorageBuffer<Sh9> Sh9 = null!;
    }
}
