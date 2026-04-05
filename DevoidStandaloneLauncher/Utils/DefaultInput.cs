using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Utils
{
    public static class DefaultInput
    {
        public static void ConfigureInput()
        {
            Input.Map.Bind("PosX", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.X,
                isClamped = false
            });

            Input.Map.Bind("PosY", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.Y,
                isClamped = false
            });

            Input.Map.Bind("LookX", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaX,
                isClamped = false
            });

            Input.Map.Bind("LookY", new InputBinding()
            {
                DeviceType = InputDeviceType.Mouse,
                Control = (ushort)MouseAxis.DeltaY,
                isClamped = false
            });

            Input.Map.Bind("Forward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.W
            });

            Input.Map.Bind("Backward", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.S
            });

            Input.Map.Bind("Left", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.A
            });

            Input.Map.Bind("Right", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.D
            });

            Input.Map.Bind("Capture", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.G
            });

            Input.Map.Bind("Up", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.Space
            });

            Input.Map.Bind("Down", new InputBinding()
            {
                DeviceType = InputDeviceType.Keyboard,
                Control = (ushort)Keys.LeftShift
            });

            Input.Backend.OnDeviceConnected += (InputDeviceType type, uint id) =>
            {
                Console.WriteLine(type);
            };
        }

    }
}
