using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    public struct Rect
    {
        public Vector2 Position;
        public Vector2 Size;

        public Rect() { }
        public Rect(Vector2 start, Vector2 end)
        {
            Position = start;
            Size = end;
        }
    }
}
