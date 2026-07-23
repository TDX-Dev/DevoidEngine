using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Audio
{
    public readonly struct AudioPlayHandle
    {
        public readonly uint Id;

        internal AudioPlayHandle(uint id)
        {
            Id = id;
        }
    }
}
