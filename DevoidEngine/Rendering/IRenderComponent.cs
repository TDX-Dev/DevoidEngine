using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public interface IRenderComponent
    {
        public void Collect(Camera camera, RenderView viewData);
    }
}
