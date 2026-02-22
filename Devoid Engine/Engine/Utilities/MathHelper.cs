namespace DevoidEngine.Engine.Utilities
{
    public class MathHelper
    {
        public const float PI = (float)Math.PI;
        public const float TwoPI = (float)(Math.PI * 2.0);

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float DegToRad(float degrees)
        {
            return degrees * (PI / 180f);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static float RadToDeg(float radians)
        {
            return radians * (180f / PI);
        }

        public static float Lerp(float a, float b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return a + (b - a) * t;
        }

        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }


    }
}
