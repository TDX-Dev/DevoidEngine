namespace DevoidEngine.Engine.InputSystem
{
    public abstract class InputDeviceHandler
    {
        public abstract void Register(InputBackend backend);
        public abstract void Update(InputBackend backend);
    }
}