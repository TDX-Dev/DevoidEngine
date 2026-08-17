using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;

namespace Elemental
{
    public sealed class EditorInputLayer : IInputLayer
    {
        public bool SceneViewFocused { get; set; }

        public bool Handle(ref InputEvent e)
        {
            if (!SceneViewFocused)
                return false;

            // When the Scene View owns input, prevent
            // gameplay input from receiving it.

            if (e.DeviceType == InputDeviceType.Keyboard)
                return true;

            if (e.DeviceType == InputDeviceType.Mouse)
                return true;

            return false;
        }
    }
}