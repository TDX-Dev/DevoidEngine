using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public struct Padding
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public readonly float Horizontal => Left + Right;
        public readonly float Vertical => Top + Bottom;

        public static Padding GetAll(float val) => new() { Left = val, Right = val, Top = val, Bottom = val };
        public static Padding GetVertical(float val) => new() { Top = val, Bottom = val };
        public static Padding GetHorizontal(float val) => new() { Left = val, Right = val };
    }
}
