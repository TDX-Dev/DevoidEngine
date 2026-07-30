using DevoidEngine.Core;
using DevoidEngine.InputSystem.InputDevices;

namespace DevoidEngine.InputSystem
{
    public class Input
    {
        public InputBackend Backend = new();
        public InputRouter Router = new();
        public InputState State = new();
        public InputMap Map = new();

        private static Window currentWindow = null!;

        public Input()
        {

        }

        public void UpdateInputProviderWindow(Window window)
        {
            currentWindow = window;
            Backend.AddInputDevice(new KeyboardInputHandler(currentWindow.KeyboardState));
            Backend.AddInputDevice(new MouseInputHandler(currentWindow.MouseState));
            Backend.AddInputDevice(new GamepadInputHandler(currentWindow.JoystickStates));

            window.OnWindowTextInput += TextInput;
        }

        public void LoadInputActions(List<InputAction> inputActions)
        {
            Map = new InputMap();

            foreach (var action in inputActions)
            {
                foreach (var binding in action.Bindings)
                {
                    Map.Bind(action.Name, binding);
                }
            }
        }

        public void Update()
        {
            Backend.UpdateInput();
            Router.Route(Backend.GetEvents(), State);
            Backend.ClearEvents();
        }

        public void EndFrame()
        {
            State.EndFrame();
        }

        public void TextInput(char input)
        {
            Backend.Emit(new InputEvent
            {
                EventType = InputEventType.Text,
                DeviceType = InputDeviceType.Keyboard,
                Character = (char)input
            });
        }

        public void AddBinding(string action, InputBinding binding)
        {
            Map.Bind(action, binding);
        }

        public float GetAction(string action)
            => Map.Evaluate(action, State);

        public bool GetActionDown(string action)
            => Map.EvaluateDown(action, State);

        public bool GetKey(Keys w)
        {
            return false;
        }
    }
}
