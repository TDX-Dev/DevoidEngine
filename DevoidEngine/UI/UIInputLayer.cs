using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    internal class UIInputLayer : IInputLayer
    {
        private Vector2 mouse;

        public bool Handle(ref InputEvent e)
        {
            if (e.DeviceType == InputDeviceType.Mouse)
            {
                if (e.ControlType == ControlType.Float)
                {
                    switch ((MouseAxis)e.Control)
                    {
                        case MouseAxis.X:
                            mouse.X = e.Value;
                            Engine.UISystem.MouseMove(mouse);
                            return false;

                        case MouseAxis.Y:
                            mouse.Y = e.Value;
                            Engine.UISystem.MouseMove(mouse);
                            return false;

                        case MouseAxis.ScrollY:
                            Engine.UISystem.MouseScroll(new Vector2(0, e.Value));
                            return true;

                        case MouseAxis.ScrollX:
                            Engine.UISystem.MouseScroll(new Vector2(e.Value, 0));
                            return true;
                    }
                }

                if (e.Control == (ushort)MouseButton.Left)
                {
                    if (e.Value > 0)
                        Engine.UISystem.MouseDown(mouse);
                    else
                        Engine.UISystem.MouseUp(mouse);

                    return true;
                }
            }

            if (e.DeviceType == InputDeviceType.Keyboard)
            {
                Keys key = (Keys)e.Control;

                if (e.Value > 0)
                    Engine.UISystem.KeyDown(key);
                else
                    Engine.UISystem.KeyUp(key);

                return false;// UISystem.FocusedNode != null;
            }


            return false;
        }
    }
}
