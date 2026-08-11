namespace DevoidEngine.InputSystem
{
    public class InputRouter
    {
        private readonly Stack<IInputLayer> _layers;

        public InputRouter()
        {
            _layers = [];
        }

        public void Push(IInputLayer layer) => _layers.Push(layer);

        //public void Route(List<InputEvent> events, InputState state)
        //{
        //    foreach (var e in events)
        //    {
        //        bool consumed = false;

        //        foreach (var layer in _layers)
        //        {
        //            if (layer.Handle(e))
        //            {
        //                consumed = true;
        //                break;
        //            }
        //        }

        //        if (!consumed)
        //            state.Apply(e);
        //    }
        //}

        public void Route(List<InputEvent> events, InputState state)
        {
            for (int i = 0; i < events.Count; i++)
            {
                InputEvent e = events[i];

                bool consumed = false;

                foreach (var layer in _layers)
                {
                    if (layer.Handle(ref e))
                    {
                        consumed = true;
                        break;
                    }
                }

                if (consumed)
                {
                    state.Reset(e);
                    continue;
                }

                state.Apply(e);
            }
        }
    }
}
