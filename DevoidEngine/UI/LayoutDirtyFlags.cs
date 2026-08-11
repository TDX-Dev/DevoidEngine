using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    [Flags]
    internal enum LayoutDirtyFlags
    {
        None = 0,
        Measure = 1 << 0,
        Arrange = 1 << 1,
        Children = 1 << 2,
    }
}
