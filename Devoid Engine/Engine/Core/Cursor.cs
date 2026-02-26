using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public static class Cursor
    {
        internal static CursorState cursorState;
        internal static bool isDirty = false;

        public static void SetCursorState(CursorState state)
        {
            cursorState = state;
            isDirty = true;
        }

    }
}
