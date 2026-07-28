using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem
{
    public struct InputEvent
    {
        public InputEventType EventType;

        public uint DeviceId;
        public InputDeviceType DeviceType; // NOT DeviceId
        public ushort Control;
        public float Value;
        public ControlType ControlType;

        public char Character;
    }
}
