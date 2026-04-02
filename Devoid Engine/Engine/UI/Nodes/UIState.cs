using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    [Flags]
    public enum UIState
    {
        Normal = 0,
        Hover = 1 << 0,
        Pressed = 1 << 1,
        Focused = 1 << 2,
        Editing = 1 << 3,
        Disabled = 1 << 4
    }
}
