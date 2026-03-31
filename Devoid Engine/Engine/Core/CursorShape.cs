using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public enum CursorShape
    {
        //
        // Summary:
        //     Default standard shape for user-created cursors.
        CustomShape = 0,
        //
        // Summary:
        //     A normal arrow cursor.
        Arrow = 1,
        //
        // Summary:
        //     An I-beam text editing cursor.
        IBeam = 2,
        //
        // Summary:
        //     A crosshair cursor.
        Crosshair = 3,
        //
        // Summary:
        //     A pointing hand cursor.
        PointingHand = 4,
        //
        // Summary:
        //     A horizontal resize cursor.
        ResizeEW = 5,
        //
        // Summary:
        //     A vertical resize cursor.
        ResizeNS = 6,
        //
        // Summary:
        //     A diagonal northwest southeast resize cursor.
        ResizeNWSE = 7,
        //
        // Summary:
        //     A diagonal northeast southwest resize cursor.
        ResizeNESW = 8,
        //
        // Summary:
        //     An omni-directional resize cursor.
        ResizeAll = 9,
        //
        // Summary:
        //     An operation-not-allowed cursor.
        NotAllowed = 10,
    }
}
