using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ProjectSystem
{
    public static class ProjectManager
    {
        public static Project Current { get; private set; }

        public static void Load(string projectFile)
        {
            Current = Project.Load(projectFile);
        }

        public static void Create(string path, string name)
        {
            Current = Project.Create(path, name);
        }

        public static bool HasProject => Current != null;
    }
}
