using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    class InputLayoutCache
    {
        private Dictionary<(VertexInfo, IShader), IInputLayout> cache = new();

        public IInputLayout Get(IGraphicsDevice device, VertexInfo layout, IShader shader)
        {
            var key = (layout, shader);

            if (!cache.TryGetValue(key, out var inputLayout))
            {
                inputLayout = device.CreateInputLayout(layout, shader);
                cache[key] = inputLayout;
            }

            return inputLayout;
        }
    }
}
