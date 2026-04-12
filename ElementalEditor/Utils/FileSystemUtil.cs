using DevoidEngine.Engine.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Utils
{
    public static class FileSystemUtil
    {
        public static string ToRelative(string absolute)
        {
            string root = ProjectManager.Current.AssetPath;

            if (absolute.StartsWith(root))
                return Path.GetRelativePath(root, absolute);

            return absolute;
        }

        public static string ToAbsolute(string relative)
        {
            if (Path.IsPathRooted(relative))
                return relative;

            return Path.Combine(ProjectManager.Current.AssetPath, relative);
        }
    }
}
