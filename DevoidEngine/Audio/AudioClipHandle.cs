using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Audio
{
    public readonly struct AudioClipHandle
    {
        public readonly uint Id;

        internal AudioClipHandle(uint id)
        {
            Id = id;
        }
    }
}
