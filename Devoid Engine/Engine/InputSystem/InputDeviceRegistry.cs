namespace DevoidEngine.Engine.InputSystem
{
    public class InputDeviceRegistry
    {
        private Dictionary<uint, InputDeviceLayout> _layouts = new();
        private uint _nextDeviceId = 0;

        public uint RegisterDevice(InputDeviceLayout layout)
        {
            uint deviceId = ++_nextDeviceId;
            _layouts[deviceId] = layout;
            return deviceId;
        }

        public string GetControlName(uint deviceId, ushort control)
        {
            if (_layouts.TryGetValue(deviceId, out var layout) &&
                layout.ControlNames.TryGetValue(control, out var name))
            {
                return name;
            }

            return $"Unknown({control})";
        }
    }
}