using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.InputSystem.InputDevices
{
    internal class MouseInputHandler : InputDeviceHandler
    {
        private readonly MouseState state;
        private uint deviceId = 0;

        private readonly MouseButton[] buttons;
        private readonly HashSet<MouseButton> previousButtons;

        private Vector2 previousPosition;
        private Vector2 previousScroll;
        public MouseInputHandler(MouseState mouseState)
        {
            state = mouseState;
            buttons = Enum.GetValues<MouseButton>();
            previousPosition = state.Position;

            previousButtons = [];
        }

        public override void Register(InputBackend backend)
        {
            InputDeviceLayout layout = new()
            {
                Name = "Mouse0",
                DeviceType = InputDeviceType.Mouse
            };

            deviceId = backend.InputDeviceRegistry.RegisterDevice(layout);

            previousScroll = state.Scroll;
        }

        public override void Update(InputBackend backend)
        {
            foreach (MouseButton button in buttons)
            {
                bool isDown = state.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
                bool wasDown = previousButtons.Contains(button);

                if (isDown && !wasDown)
                {
                    backend.Emit(new InputEvent
                    {
                        DeviceId = deviceId,
                        DeviceType = InputDeviceType.Mouse,
                        Control = (ushort)button,
                        Value = 1f,
                        ControlType = ControlType.Bool
                    });

                    previousButtons.Add(button);
                }
                else if (!isDown && wasDown)
                {
                    backend.Emit(new InputEvent
                    {
                        DeviceId = deviceId,
                        DeviceType = InputDeviceType.Mouse,
                        Control = (ushort)button,
                        Value = 0f,
                        ControlType = ControlType.Bool
                    });

                    previousButtons.Remove(button);
                }
            }

            Vector2 currentPos = state.Position;
            Vector2 delta = currentPos - previousPosition;

            // scale it down (VERY IMPORTANT)
            //delta *= 0.002f;

            backend.Emit(new InputEvent
            {
                DeviceId = deviceId,
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaX,
                Value = delta.X
            });

            backend.Emit(new InputEvent
            {
                DeviceId = deviceId,
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaY,
                Value = delta.Y
            });

            previousPosition = currentPos;

            Vector2 pos = state.Position;

            backend.Emit(new InputEvent
            {
                DeviceId = deviceId,
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.X,
                Value = pos.X
            });

            backend.Emit(new InputEvent
            {
                DeviceId = deviceId,
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.Y,
                Value = pos.Y
            });


            previousPosition = currentPos;

            Vector2 scroll = state.Scroll;
            Vector2 scrollDelta = state.ScrollDelta;

            backend.Emit(new InputEvent
            {
                DeviceId = deviceId,
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.ScrollX,
                Value = scrollDelta.X
            });

            backend.Emit(new InputEvent
            {
                DeviceId = deviceId,
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.ScrollY,
                Value = scrollDelta.Y
            });

            previousScroll = scroll;
        }
    }
}
