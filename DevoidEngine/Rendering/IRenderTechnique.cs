using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public interface IRenderTechnique : IDisposable
    {
        void Initialize();
        void Render(CameraRenderContext ctx);
    }
}
