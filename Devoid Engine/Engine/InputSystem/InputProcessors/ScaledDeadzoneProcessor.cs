namespace DevoidEngine.Engine.InputSystem.InputProcessors
{
    public class ScaledDeadzoneProcessor : IInputProcessor
    {
        public float Deadzone { get; set; }

        public ScaledDeadzoneProcessor(float deadzone)
        {
            Deadzone = deadzone;
        }

        public float Process(float value)
        {
            float abs = MathF.Abs(value);

            if (abs < Deadzone)
                return 0f;

            float sign = MathF.Sign(value);
            float scaled = (abs - Deadzone) / (1f - Deadzone);

            return scaled * sign;
        }
    }
}