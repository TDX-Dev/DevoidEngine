namespace DevoidEngine.Engine.InputSystem
{
    public class InputBinding
    {
        public InputDeviceType DeviceType { get; set; }
        public ushort Control { get; set; }

        public float Scale { get; set; } = 1f;
        public bool IsClamped { get; set; } = true;

        public List<IInputProcessor> Processors { get; set; } = new();
    }
}