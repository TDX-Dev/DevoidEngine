namespace DevoidEngine.InputSystem
{
    public class InputAction
    {
        public string Name { get; set; } = "";
        public List<InputBinding> Bindings { get; set; }

        public InputAction()
        {
            Bindings = [];
        }
    }
}
