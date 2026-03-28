namespace DevoidEngine.Engine.InputSystem
{
    public struct InputEvent
    {
        public uint DeviceId;
        public InputDeviceType DeviceType; // NOT DeviceId
        public ushort Control;
        public float Value;
    }
}