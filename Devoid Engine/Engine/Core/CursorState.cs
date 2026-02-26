using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public enum CursorState
    {
        //
        // Summary:
        //     The cursor visible and cursor motion is not limited.
        Normal,
        //
        // Summary:
        //     Hides the cursor when over a window.
        Hidden,
        //
        // Summary:
        //     Hides the cursor and locks it to the specified window.
        Grabbed,
        //
        // Summary:
        //     Confines the cursor to the window content area.
        Confined
    }
}
