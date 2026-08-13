using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    internal sealed class GizmoInputLayer : IInputLayer
    {
        private Vector2 mouse;

        public bool Handle(ref InputEvent e)
        {
            if (e.DeviceType != InputDeviceType.Mouse)
                return false;

            if (e.ControlType == ControlType.Float)
            {
                switch ((MouseAxis)e.Control)
                {
                    case MouseAxis.X:
                        mouse.X = e.Value;
                        Engine.GizmoSystem.MouseMove(mouse);
                        return false;

                    case MouseAxis.Y:
                        mouse.Y = e.Value;
                        Engine.GizmoSystem.MouseMove(mouse);
                        return false;

                    case MouseAxis.DeltaX:
                    case MouseAxis.DeltaY:
                        return false;

                    case MouseAxis.ScrollX:
                    case MouseAxis.ScrollY:
                        return false;
                }
            }

            if (e.Control == (ushort)MouseButton.Left)
            {
                if (e.Value > 0)
                    return Engine.GizmoSystem.MouseDown(mouse);

                return Engine.GizmoSystem.MouseUp(mouse);
            }

            return false;
        }
    }
}