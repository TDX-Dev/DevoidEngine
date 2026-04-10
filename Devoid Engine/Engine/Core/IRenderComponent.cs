using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public interface IRenderComponent
    {
        public void Collect(Camera camera, CameraRenderContext viewData);
    }
}
