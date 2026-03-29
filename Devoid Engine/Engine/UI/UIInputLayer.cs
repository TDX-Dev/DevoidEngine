using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI
{
    public class UIInputLayer : IInputLayer
    {
        private Vector2 mouse;
        private bool dragging;

        public bool Handle(InputEvent e)
        {
            if (e.DeviceType != InputDeviceType.Mouse)
                return false;

            switch ((MouseAxis)e.Control)
            {
                case MouseAxis.X:
                    mouse.X = e.Value;
                    break;

                case MouseAxis.Y:
                    mouse.Y = e.Value;
                    break;
            }

            if (e.Control == (ushort)MouseButton.Left)
            {
                if (e.Value == 1f)
                {
                    UISystem.MouseDown(mouse);
                    return true;
                }
                else
                {
                    UISystem.MouseUp(mouse);
                    return true;
                }
            }

            if (e.Control == (ushort)MouseAxis.DeltaX ||
                e.Control == (ushort)MouseAxis.DeltaY)
            {
                UISystem.MouseMove(mouse);
                return true;
            }

            return false;
        }
    }
}
