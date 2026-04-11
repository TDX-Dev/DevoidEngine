using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem.InputDevices;
using System.Numerics;

namespace DevoidEngine.Engine.InputSystem
{
    public static class Input
    {
        public static InputBackend Backend = new();
        public static InputRouter Router = new();
        public static InputState State = new();
        public static InputMap Map = new();

        private static Window currentWindow = null!;

        public static void LoadInputActions(List<InputAction> inputActions)
        {
            Input.Map = new InputMap();

            foreach (var action in inputActions)
            {
                foreach (var binding in action.Bindings)
                {
                    Input.Map.Bind(action.Name, binding);
                }
            }
        }

        public static void Initialize(Window window)
        {
            currentWindow = window;
            Backend.AddInputDevice(new InputDevices.KeyboardInputHandler(currentWindow.KeyboardState));
            Backend.AddInputDevice(new InputDevices.MouseInputHandler(currentWindow.MouseState));
            Backend.AddInputDevice(new InputDevices.GamepadInputHandler(currentWindow.JoystickStates));
        }

        public static void Update()
        {
            Backend.UpdateInput();
            Router.Route(Backend.GetEvents(), State);
        }

        public static void EndFrame()
        {
            State.EndFrame();
        }

        public static float GetAction(string action)
            => Map.Evaluate(action, State);

        public static bool GetActionDown(string action)
            => Map.EvaluateDown(action, State);

        public static bool GetKey(Keys w)
        {
            return false;
        }
    }
}