using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public enum UICommandType : byte
    {
        PushTransform,
        PopTransform,
        Quad,
        Text,
        PushClip,
        PopClip,
    }
}
