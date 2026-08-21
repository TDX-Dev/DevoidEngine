using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public interface IGizmoProviderComponent
    {
        void OnDrawGizmos(GizmoContext context);
    }
}
