using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public class DirectorySource : IVirtualFileSource
    {
        private readonly string root;

        public DirectorySource(string root)
        {
            this.root = root;
        }

        public bool Exists(string path)
        {
            return File.Exists(Path.Combine(root, path));
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(Path.Combine(root, path));
        }

        public IEnumerable<string> Enumerate(string path)
        {
            var dir = Path.Combine(root, path);

            if (!Directory.Exists(dir))
                return Enumerable.Empty<string>();

            return Directory.GetFiles(dir);
        }
    }
}
