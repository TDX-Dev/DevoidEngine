namespace DevoidEngine.InputSystem
{
    public class InputDeviceRegistry
    {
        private readonly Dictionary<uint, InputDeviceLayout> _layouts;
        private uint _nextDeviceId = 0;

        public InputDeviceRegistry()
        {
            _layouts = [];
        }

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
