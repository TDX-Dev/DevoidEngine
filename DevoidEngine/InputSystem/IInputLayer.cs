namespace DevoidEngine.InputSystem
{
    public interface IInputLayer
    {
        bool Handle(ref InputEvent e); // true = consume
    }
}
