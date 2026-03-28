namespace DevoidEngine.Engine.InputSystem
{
    public class InputBinding
    {
        public InputDeviceType DeviceType;
        public ushort Control;

        public float Scale = 1f;
        public bool isClamped = true;
        public List<IInputProcessor> Processors = new();
    }
}