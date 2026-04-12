using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.GizmoSystem
{
    [Flags]
    public enum GizmoCategory
    {
        None = 0,
        Lighting = 1 << 0,
        Physics = 1 << 1,
        Cameras = 1 << 2,
        Audio = 1 << 3,
        AI = 1 << 4,
        Gameplay = 1 << 5,
        Navigation = 1 << 6,
        Custom = 1 << 7,

        All = ~0
    }
}
