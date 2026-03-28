namespace DevoidEngine.Engine.InputSystem
{
    public class InputDeviceLayout
    {
        public string Name;
        public InputDeviceType DeviceType;
        public Dictionary<ushort, string> ControlNames = new();
    }

    public enum ControlKind
    {
        Button,
        Axis,
        Delta
    }
}