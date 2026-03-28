namespace DevoidEngine.Engine.InputSystem
{
    public class InputBackend
    {
        public InputDeviceRegistry InputDeviceRegistry { get; set; }
        public event Action<InputDeviceType, uint> OnDeviceConnected;
        public event Action<InputDeviceType, uint> OnDeviceDisconnected;

        List<InputDeviceHandler> _inputDeviceHandlers;
        List<InputEvent> _events;

        public InputBackend()
        {
            _inputDeviceHandlers = new List<InputDeviceHandler>();
            _events = new List<InputEvent>();
        }

        public void AddInputDevice(InputDeviceHandler deviceHandler)
        {
            _inputDeviceHandlers.Add(deviceHandler);
        }

        public void NotifyDeviceConnected(InputDeviceType type, uint deviceId)
        {
            OnDeviceConnected?.Invoke(type, deviceId);
        }

        public void NotifyDeviceDisconnected(InputDeviceType type, uint deviceId)
        {
            OnDeviceDisconnected?.Invoke(type, deviceId);
        }

        public void Emit(InputEvent e)
        {
            _events.Add(e);
        }

        public List<InputEvent> GetEvents()
        {
            return _events;
        }

        public void UpdateInput()
        {
            _events.Clear();
            for (int i = 0; i < _inputDeviceHandlers.Count; i++)
            {
                InputDeviceHandler handler = _inputDeviceHandlers[i];
                handler.Update(this);
            }
        }
    }
}