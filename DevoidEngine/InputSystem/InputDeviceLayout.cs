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
