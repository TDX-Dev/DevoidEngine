using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public static class UtilRandom
    {
        public static readonly Random Instance = new Random();

        public static long GetInt()
        {
            return Instance.NextInt64();
        }

        public static long GetInt(int maxValue)
        {
            return Instance.NextInt64(maxValue);
        }

        public static long GetInt(int minValue, int maxValue)
        {
            return Instance.NextInt64(minValue, maxValue);
        }

        public static int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        public static int Next()
        {
            return Instance.Next();
        }
    }
}
