using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class ShaderStage
    {
        public IShader GPU { get; }

        public ShaderStage(IShader shader)
        {
            GPU = shader;
        }
    }
}
