using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Inspector
{
    public static class InspectorTabRegistry
    {
        static readonly List<IInspectorTab> tabs = new();

        public static void Register(IInspectorTab tab)
        {
            tabs.Add(tab);
        }

        public static IReadOnlyList<IInspectorTab> Tabs => tabs;
    }
}
