using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.Engine.InputSystem.InputDevices
{
    internal class KeyboardInputHandler : InputDeviceHandler
    {
        private KeyboardState _state;
        private uint _deviceId = 0;
        private Keys[] Keys;

        public KeyboardInputHandler(KeyboardState state)
        {
            _state = state;
            Keys = Enum.GetValues<Keys>();
        }

        public override void Register(InputBackend backend)
        {
            InputDeviceLayout layout = new InputDeviceLayout();
            layout.Name = "Keyboard0";
            layout.DeviceType = InputDeviceType.Keyboard;
            _deviceId = backend.InputDeviceRegistry.RegisterDevice(layout);
        }

        private HashSet<Keys> _previous = new();

        public override void Update(InputBackend backend)
        {

            foreach (Keys key in Keys)
            {
                if (key == InputDevices.Keys.Unknown) continue;
                var Okey = (OpenTK.Windowing.GraphicsLibraryFramework.Keys)key;
                bool isDown = _state.IsKeyDown(Okey);
                bool wasDown = _previous.Contains(key);

                if (isDown && !wasDown)
                {
                    backend.Emit(new InputEvent
                    {
                        DeviceId = _deviceId,
                        DeviceType = InputDeviceType.Keyboard,
                        Control = (ushort)key,
                        Value = 1f
                    });

                    _previous.Add(key);
                }
                else if (!isDown && wasDown)
                {
                    backend.Emit(new InputEvent
                    {
                        DeviceId = _deviceId,
                        DeviceType = InputDeviceType.Keyboard,
                        Control = (ushort)key,
                        Value = 0f
                    });

                    _previous.Remove(key);
                }
            }
        }
    }
}