using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public struct KeyboardEvent
    {
        public Keys Key;

        public int scanCode;
        public string modifiers;

        public bool Caps;
    }

    public struct TextInputEvent
    {
        public int Unicode;
    }

    internal class Input
    {
    }
}
