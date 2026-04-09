namespace DevoidEngine.Engine.InputSystem
{
    public interface IInputLayer
    {
        bool Handle(ref InputEvent e); // true = consume
    }
}