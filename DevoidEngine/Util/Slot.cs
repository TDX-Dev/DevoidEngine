using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Util
{
    internal struct Slot<T>
    {
        public T Value;
        public uint Generation;
        public bool Occupied;
    }
}
