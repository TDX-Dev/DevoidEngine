using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidRuntime
{
    public static class ScriptLoader
    {
        public static void LoadScripts()
        {
#if SCRIPT_DYNAMIC
        DynamicScriptLoader.Load();
#elif SCRIPT_STATIC
        StaticScriptLoader.Load();
#endif
        }
    }
}
