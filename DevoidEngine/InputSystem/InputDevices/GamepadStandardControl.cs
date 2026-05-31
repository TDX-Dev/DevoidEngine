using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem.InputDevices
{
    public enum GamepadStandardControl
    {
        South, // A / Cross
        East,  // B / Circle
        West,  // X / Square
        North, // Y / Triangle

        LeftShoulder,
        RightShoulder,

        LeftTrigger,
        RightTrigger,

        LeftStickX,
        LeftStickY,

        RightStickX,
        RightStickY,

        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight,

        Start,
        Select
    }
}
