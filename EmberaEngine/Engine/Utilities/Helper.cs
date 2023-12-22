using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public static class Helper
    {
        public static System.Numerics.Vector2 ToNumerics2(OpenTK.Mathematics.Vector2 value)
        {
            return Unsafe.As<OpenTK.Mathematics.Vector2, System.Numerics.Vector2>(ref value);
        }

        public static OpenTK.Mathematics.Vector2 ToOpenTK(this System.Numerics.Vector2 vector4)
        {
            return Unsafe.As<System.Numerics.Vector2, OpenTK.Mathematics.Vector2>(ref vector4);
        }

    }
}
