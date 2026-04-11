using Assimp;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using System.Numerics;

namespace ElementalEditor.Utils
{
    public class EditorInputLayer : IInputLayer
    {
        EditorCamera camera;

        bool rightDown;
        bool shiftDown;

        public bool ViewportActive = false;
        bool forward;
        bool backward;
        bool left;
        bool right;
        bool up;
        bool down;

        public bool IsNavigating => rightDown;

        public EditorInputLayer(EditorCamera cam)
        {
            camera = cam;
        }

        public void Update(float dt)
        {
            if (!rightDown)
            {
                forward = backward = left = right = up = down = false;
                return;
            }

            Vector3 move = Vector3.Zero;

            Vector3 forwardDir = camera.GetForward();
            Vector3 rightDir = camera.GetRight();
            Vector3 upDir = camera.GetUp();

            if (forward) move += forwardDir;
            if (backward) move -= forwardDir;
            if (right) move += rightDir;
            if (left) move -= rightDir;
            if (up) move += upDir;
            if (down) move -= upDir;

            if (move.LengthSquared() > 0)
            {
                move = Vector3.Normalize(move);
                camera.Position += move * camera.MoveSpeed * dt;
                camera.UpdateView();
            }
        }

        public bool Handle(ref InputEvent e)
        {
            if (!ViewportActive && !rightDown)
                return false;

            if (e.DeviceType == InputDeviceType.Keyboard)
            {
                if (!rightDown)
                    return false;

                if (e.Control == (ushort)Keys.W)
                    forward = e.Value > 0;

                if (e.Control == (ushort)Keys.S)
                    backward = e.Value > 0;

                if (e.Control == (ushort)Keys.A)
                    left = e.Value > 0;

                if (e.Control == (ushort)Keys.D)
                    right = e.Value > 0;
                if (e.Control == (ushort)Keys.LeftShift)
                    shiftDown = e.Value > 0;
            }

            if (e.DeviceType == InputDeviceType.Mouse)
            {
                // right mouse button
                if (e.Control == (ushort)MouseButton.Right && e.ControlType == ControlType.Bool)
                {
                    rightDown = e.Value > 0;

                    if (rightDown)
                    {
                        Cursor.SetCursorState(CursorState.Grabbed);

                        if (!shiftDown)
                            camera.StartRotate();
                        else
                            camera.StartPan();
                    }
                    else
                    {
                        Cursor.SetCursorState(CursorState.Normal);

                        camera.StopRotate();
                        camera.StopPan();
                    }
                }

                if (rightDown)
                {
                    if (e.Control == (ushort)MouseAxis.DeltaX)
                        camera.MouseDelta(e.Value, 0);

                    if (e.Control == (ushort)MouseAxis.DeltaY)
                        camera.MouseDelta(0, e.Value);
                }

                if (e.Control == (ushort)MouseAxis.ScrollY)
                    camera.Scroll(e.Value);

                return true;
            }

            return false;
        }
    }
}