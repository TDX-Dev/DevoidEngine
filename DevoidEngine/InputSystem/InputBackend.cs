using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem
{
    public class InputBackend
    {
        public InputDeviceRegistry InputDeviceRegistry { get; set; } = null!;
        public event Action<InputDeviceType, uint>? OnDeviceConnected;
        public event Action<InputDeviceType, uint>? OnDeviceDisconnected;

        private readonly List<InputDeviceHandler> _inputDeviceHandlers;
        private readonly List<InputEvent> _events;

        public InputBackend()
        {
            _inputDeviceHandlers = [];
            _events = [];
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
