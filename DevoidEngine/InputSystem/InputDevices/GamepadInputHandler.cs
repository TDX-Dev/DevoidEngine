using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.InputSystem.InputDevices
{
    internal class GamepadInputHandler : InputDeviceHandler
    {
        private readonly IReadOnlyList<JoystickState> states;
        private readonly Dictionary<(int device, int button), bool> prevButtons;

        private readonly HashSet<int> connectedLastFrame;
        private readonly HashSet<int> currentConnected;

        private readonly Dictionary<(int device, GamepadStandardControl), bool> prevHatStates;

        private static readonly Dictionary<int, GamepadStandardControl> buttonMap = new()
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

        public GamepadInputHandler(IReadOnlyList<JoystickState> joystickstates)
        {
            states = joystickstates;
            prevButtons = [];
            prevHatStates = [];
            connectedLastFrame = [];
            currentConnected = [];
        }

        public override void Register(InputBackend backend)
        {

        }

        public override void Update(InputBackend backend)
        {

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

                if (!connectedLastFrame.Contains(i))
                {
                    backend.NotifyDeviceConnected(InputDeviceType.Gamepad, (uint)i);
                }

                for (int j = 0; j < state.ButtonCount; j++)
                {
                    bool isDown = state.IsButtonDown(j);
                    var key = (i, j);

                    bool wasDown = prevButtons.TryGetValue(key, out var prev) && prev;

                    ushort control = buttonMap.TryGetValue(j, out var mapped)
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

                        prevButtons[key] = true;
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

                        prevButtons[key] = false;
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

                for (int h = 0; h < state.HatCount; h++)
                {
                    var hat = (GamepadHats)state.GetHat(h);

                    EmitHat(backend, i, GamepadStandardControl.DpadUp, (hat & GamepadHats.Up) != 0);
                    EmitHat(backend, i, GamepadStandardControl.DpadDown, (hat & GamepadHats.Down) != 0);
                    EmitHat(backend, i, GamepadStandardControl.DpadLeft, (hat & GamepadHats.Left) != 0);
                    EmitHat(backend, i, GamepadStandardControl.DpadRight, (hat & GamepadHats.Right) != 0);
                }
            }
            foreach (var old in connectedLastFrame)
            {
                if (!currentConnected.Contains(old))
                {
                    backend.NotifyDeviceDisconnected(InputDeviceType.Gamepad, (uint)old);
                }
            }

            connectedLastFrame.Clear();
            foreach (var c in currentConnected)
                connectedLastFrame.Add(c);
        }

        private void EmitHat(InputBackend backend, int device, GamepadStandardControl controlEnum, bool isDown)
        {
            var key = (device, controlEnum);
            bool wasDown = prevHatStates.TryGetValue(key, out var prev) && prev;

            if (isDown && !wasDown)
            {
                backend.Emit(new InputEvent
                {
                    DeviceId = (uint)device,
                    DeviceType = InputDeviceType.Gamepad,
                    Control = (ushort)controlEnum,
                    Value = 1f
                });

                prevHatStates[key] = true;
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

                prevHatStates[key] = false;
            }
        }
    }
}
