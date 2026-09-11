namespace DevoidEngine.InputSystem.InputDevices
{
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Ctrl = 1 << 0,
        Shift = 1 << 1,
        Alt = 1 << 2,
        Super = 1 << 3
    }
}
