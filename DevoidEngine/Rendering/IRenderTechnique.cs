using DevoidEngine.Core;
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
        RenderTarget Render(RenderContext ctx, RenderView view);
    }
}
