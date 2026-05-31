using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem
{
    public class InputState
    {
        private readonly Dictionary<(InputDeviceType, ushort), float> _values;
        private readonly HashSet<(InputDeviceType, ushort)> _pressed;
        private readonly HashSet<(InputDeviceType, ushort)> _released;

        public InputState()
        {
            _values = [];
            _pressed = [];
            _released = [];
        }

        public void Apply(InputEvent e)
        {
            var key = (e.DeviceType, e.Control);

            float prev = Get(e.DeviceType, e.Control);

            if (prev == 0 && e.Value != 0)
                _pressed.Add(key);

            if (prev != 0 && e.Value == 0)
                _released.Add(key);

            _values[key] = e.Value;
        }

        public float Get(InputDeviceType type, ushort control)
        {
            return _values.TryGetValue((type, control), out var v) ? v : 0f;
        }

        public bool GetDown(InputDeviceType type, ushort control)
            => _pressed.Contains((type, control));

        public void EndFrame()
        {
            _pressed.Clear();
            _released.Clear();
        }

        public void Clear()
        {
            _values.Clear();
            _pressed.Clear();
            _released.Clear();
        }
    }
}
