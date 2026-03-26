using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class VirtualFileSystem
    {
        private readonly List<IVirtualFileSource> sources = new();

        public void Mount(IVirtualFileSource source)
        {
            sources.Insert(0, source); // higher priority first
        }

        public bool Exists(string path)
        {
            foreach (var source in sources)
            {
                if (source.Exists(path))
                    return true;
            }
            return false;
        }

        public Stream OpenRead(string path)
        {
            foreach (var source in sources)
            {
                if (source.Exists(path))
                    return source.OpenRead(path);
            }

            throw new FileNotFoundException($"VFS: File not found: {path}");
        }

        public byte[] ReadAllBytes(string path)
        {
            using var stream = OpenRead(path);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public string ReadAllText(string path)
        {
            using var stream = OpenRead(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
