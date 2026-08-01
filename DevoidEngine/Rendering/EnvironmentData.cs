using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EnvironmentData
    {
        SH9 SkySHIrradiance;
    }
}
