using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Util
{
    public static class InspectorTabRegistry
    {
        static readonly List<IInspectorTab> tabs = [];

        public static void Register(IInspectorTab tab)
        {
            tabs.Add(tab);
        }

        public static IReadOnlyList<IInspectorTab> Tabs => tabs;
    }
}
