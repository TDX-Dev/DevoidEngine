namespace DevoidEngine.Engine.InputSystem
{
    public class InputMap
    {
        private Dictionary<string, List<InputBinding>> _bindings = new();

        public void Bind(string action, InputBinding binding)
        {
            if (!_bindings.ContainsKey(action))
                _bindings[action] = new List<InputBinding>();

            _bindings[action].Add(binding);
        }

        public float Evaluate(string action, InputState state)
        {
            if (!_bindings.TryGetValue(action, out var list))
                return 0f;

            float value = 0f;

            foreach (var b in list)
            {
                float raw = state.Get(b.DeviceType, b.Control);

                foreach (var p in b.Processors)
                    raw = p.Process(raw);

                if (b.isClamped)
                {
                    value += Math.Clamp(raw * b.Scale, -1, 1);
                } else
                {
                    value += raw * b.Scale;
                }
            }

            return value;
        }

        public bool EvaluateDown(string action, InputState state)
        {
            if (!_bindings.TryGetValue(action, out var list))
                return false;

            foreach (var b in list)
            {
                if (state.GetDown(b.DeviceType, b.Control))
                    return true;
            }

            return false;
        }
    }
}