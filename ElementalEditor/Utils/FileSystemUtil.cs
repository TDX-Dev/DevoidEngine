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

        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo source = new DirectoryInfo(sourceDir);

            if (!source.Exists)
                throw new DirectoryNotFoundException(sourceDir);

            Directory.CreateDirectory(destinationDir);

            // copy files
            foreach (FileInfo file in source.GetFiles())
            {
                string target = Path.Combine(destinationDir, file.Name);
                file.CopyTo(target, true);
            }

            // copy subdirectories
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                string target = Path.Combine(destinationDir, dir.Name);
                CopyDirectory(dir.FullName, target);
            }
        }
    }
}
