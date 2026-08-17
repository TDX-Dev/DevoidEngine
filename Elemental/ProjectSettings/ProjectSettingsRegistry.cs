using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.ProjectSettings
{
    public static class ProjectSettingsRegistry
    {
        static readonly List<IProjectSettingsProvider> providers = [];

        public static void Register(IProjectSettingsProvider provider)
        {
            providers.Add(provider);
        }

        public static IReadOnlyList<IProjectSettingsProvider> Providers => providers;
    }
}
