using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI
{
    public class UIInputLayer : IInputLayer
    {
        private Vector2 mouse;

        public bool Handle(InputEvent e)
        {
            
            if (e.DeviceType != InputDeviceType.Mouse)
                return false;

            if (e.ControlType == ControlType.Float)
            {
                switch ((MouseAxis)e.Control)
                {
                    case MouseAxis.X:
                        mouse.X = e.Value;
                        UISystem.MouseMove(mouse);
                        return false;

                    case MouseAxis.Y:
                        mouse.Y = e.Value;
                        UISystem.MouseMove(mouse);
                        return false;

                    case MouseAxis.ScrollY:
                        UISystem.MouseScroll(new Vector2(0, e.Value));
                        return true;
                    case MouseAxis.ScrollX:
                        UISystem.MouseScroll(new Vector2(e.Value, 0));
                        return true;
                }
            }

            if (e.Control == (ushort)MouseButton.Left)
            {
                if (e.Value > 0)
                    UISystem.MouseDown(mouse);
                else
                    UISystem.MouseUp(mouse);

                return true;
            }

            return false;
        }
    }
}
