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

        public readonly bool Contains(Vector2 value)
        {
            if (value.X > Position.X && value.Y > Position.Y)
            {
                if (value.X < Size.X &&  value.Y < Size.Y)
                {
                    return true;
                }
            }
            return false;

        }
    }
}
