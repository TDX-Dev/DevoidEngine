namespace DevoidEngine.Engine.InputSystem
{
    public interface IInputLayer
    {
        bool Handle(InputEvent e); // true = consume
    }
}