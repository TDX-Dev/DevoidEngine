using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.Engine.InputSystem.InputDevices
{
    internal class GamepadInputHandler : InputDeviceHandler
    {
        private IReadOnlyList<JoystickState> _states;
        Dictionary<(int device, int button), bool> _prevButtons = new();

        HashSet<int> _connectedLastFrame = new();
        HashSet<int> currentConnected = new();

        private static readonly Dictionary<int, GamepadStandardControl> _buttonMap = new()
        {
            { 0, GamepadStandardControl.South },
            { 1, GamepadStandardControl.East },
            { 2, GamepadStandardControl.West },
            { 3, GamepadStandardControl.North },

            { 4, GamepadStandardControl.LeftShoulder },
            { 5, GamepadStandardControl.RightShoulder },

            { 6, GamepadStandardControl.Select },
            { 7, GamepadStandardControl.Start },

            { 8, GamepadStandardControl.LeftStickX },
            { 9, GamepadStandardControl.RightStickX }
        };

        public GamepadInputHandler(IReadOnlyList<JoystickState> states)
        {
            _states = states;
        }

        public override void Register(InputBackend backend)
        {

        }

        public override void Update(InputBackend backend)
        {
            var states = _states; // ideally this should be a getter, but leaving as-is for now

            currentConnected.Clear(); // IMPORTANT

            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];

                if (state == null)
                    continue;

                bool isValid =
                    state.AxisCount > 0 ||
                    state.ButtonCount > 0 ||
                    state.HatCount > 0;

                if (!isValid)
                    continue;

                currentConnected.Add(i);

                // ---- CONNECT ----
                if (!_connectedLastFrame.Contains(i))
                {
                    backend.NotifyDeviceConnected(InputDeviceType.Gamepad, (uint)i);
                }

                // =========================
                // INPUT PROCESSING
                // =========================

                // ---- BUTTONS ----
                for (int j = 0; j < state.ButtonCount; j++)
                {
                    bool isDown = state.IsButtonDown(j);
                    var key = (i, j);

                    bool wasDown = _prevButtons.TryGetValue(key, out var prev) && prev;

                    ushort control = _buttonMap.TryGetValue(j, out var mapped)
                        ? (ushort)mapped
                        : (ushort)(5000 + j);

                    if (isDown && !wasDown)
                    {
                        backend.Emit(new InputEvent
                        {
                            DeviceId = (uint)i,
                            DeviceType = InputDeviceType.Gamepad,
                            Control = control,
                            Value = 1f
                        });

                        _prevButtons[key] = true;
                    }
                    else if (!isDown && wasDown)
                    {
                        backend.Emit(new InputEvent
                        {
                            DeviceId = (uint)i,
                            DeviceType = InputDeviceType.Gamepad,
                            Control = control,
                            Value = 0f
                        });

                        _prevButtons[key] = false;
                    }
                }

                // ---- AXES ----
                for (int j = 0; j < state.AxisCount; j++)
                {
                    float value = state.GetAxis(j);

                    ushort control = j switch
                    {
                        0 => (ushort)GamepadStandardControl.LeftStickX,
                        1 => (ushort)GamepadStandardControl.LeftStickY,
                        2 => (ushort)GamepadStandardControl.RightStickX,
                        3 => (ushort)GamepadStandardControl.RightStickY,
                        4 => (ushort)GamepadStandardControl.LeftTrigger,
                        5 => (ushort)GamepadStandardControl.RightTrigger,
                        _ => (ushort)(6000 + j)
                    };

                    backend.Emit(new InputEvent
                    {
                        DeviceId = (uint)i,
                        DeviceType = InputDeviceType.Gamepad,
                        Control = control,
                        Value = value
                    });
                }

                // ---- HATS ----
                for (int h = 0; h < state.HatCount; h++)
                {
                    var hat = (GamepadHats)state.GetHat(h);

                    EmitHat(backend, i, GamepadStandardControl.DpadUp, (hat & GamepadHats.Up) != 0);
                    EmitHat(backend, i, GamepadStandardControl.DpadDown, (hat & GamepadHats.Down) != 0);
                    EmitHat(backend, i, GamepadStandardControl.DpadLeft, (hat & GamepadHats.Left) != 0);
                    EmitHat(backend, i, GamepadStandardControl.DpadRight, (hat & GamepadHats.Right) != 0);
                }
            }

            // ---- DISCONNECT ----
            foreach (var old in _connectedLastFrame)
            {
                if (!currentConnected.Contains(old))
                {
                    backend.NotifyDeviceDisconnected(InputDeviceType.Gamepad, (uint)old);
                }
            }

            // swap sets (avoid realloc)
            _connectedLastFrame.Clear();
            foreach (var c in currentConnected)
                _connectedLastFrame.Add(c);
        }

        Dictionary<(int device, GamepadStandardControl), bool> _prevHatStates = new();

        private void EmitHat(InputBackend backend, int device, GamepadStandardControl controlEnum, bool isDown)
        {
            var key = (device, controlEnum);
            bool wasDown = _prevHatStates.TryGetValue(key, out var prev) && prev;

            if (isDown && !wasDown)
            {
                backend.Emit(new InputEvent
                {
                    DeviceId = (uint)device,
                    DeviceType = InputDeviceType.Gamepad,
                    Control = (ushort)controlEnum,
                    Value = 1f
                });

                _prevHatStates[key] = true;
            }
            else if (!isDown && wasDown)
            {
                backend.Emit(new InputEvent
                {
                    DeviceId = (uint)device,
                    DeviceType = InputDeviceType.Gamepad,
                    Control = (ushort)controlEnum,
                    Value = 0f
                });

                _prevHatStates[key] = false;
            }
        }
    }
}