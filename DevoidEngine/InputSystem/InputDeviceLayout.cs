using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem
{
    public class InputDeviceLayout
    {
        public string Name = "";
        public InputDeviceType DeviceType;
        public Dictionary<ushort, string> ControlNames;

        public InputDeviceLayout()
        {
            ControlNames = [];
        }
    }

    public enum ControlKind
    {
        Button,
        Axis,
        Delta
    }
}
