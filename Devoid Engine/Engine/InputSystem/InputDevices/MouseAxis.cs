using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.InputSystem.InputDevices
{
    public enum MouseAxis : ushort
    {
        X = 0,
        Y = 1,

        DeltaX = 100,
        DeltaY = 101,

        ScrollX = 200,
        ScrollY = 201
    }
}
