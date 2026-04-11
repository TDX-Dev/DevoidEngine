using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.ProjectSettings
{
    public static class ProjectSettingsRegistry
    {
        static List<IProjectSettingsProvider> providers = new();

        public static void Register(IProjectSettingsProvider provider)
        {
            providers.Add(provider);
        }

        public static IReadOnlyList<IProjectSettingsProvider> Providers => providers;
    }
}
