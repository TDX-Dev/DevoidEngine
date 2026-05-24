using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    public static class MathHelper
    {
        public const float PI = (float)Math.PI;
        public const float TwoPI = (float)(Math.PI * 2.0);

        public static float DegToRad(float degrees)
        {
            return degrees * (PI / 180f);
        }
        public static float RadToDeg(float radians)
        {
            return radians * (180f / PI);
        }
    }
}
