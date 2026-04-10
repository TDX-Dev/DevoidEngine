using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;

namespace ElementalEditor.Utils
{
    public class EditorInputLayer : IInputLayer
    {
        EditorCamera camera;

        bool rightDown;
        bool shiftDown;

        public EditorInputLayer(EditorCamera cam)
        {
            camera = cam;
        }

        public bool Handle(ref InputEvent e)
        {
            if (e.DeviceType == InputDeviceType.Keyboard)
            {
                if (e.Control == (ushort)Keys.LeftShift)
                    shiftDown = e.Value > 0;
            }

            if (e.DeviceType == InputDeviceType.Mouse)
            {
                // right mouse button
                if (e.Control == (ushort)MouseButton.Right && e.ControlType == ControlType.Bool)
                {
                    rightDown = e.Value > 0;

                    if (rightDown && !shiftDown)
                        camera.StartRotate();

                    if (rightDown && shiftDown)
                        camera.StartPan();

                    if (!rightDown)
                    {
                        camera.StopRotate();
                        camera.StopPan();
                    }
                }

                if (e.Control == (ushort)MouseAxis.DeltaX)
                    camera.MouseDelta(e.Value, 0);

                if (e.Control == (ushort)MouseAxis.DeltaY)
                    camera.MouseDelta(0, e.Value);

                if (e.Control == (ushort)MouseAxis.ScrollY)
                    camera.Scroll(e.Value);

                return true;
            }

            return false;
        }
    }
}