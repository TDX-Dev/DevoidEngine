using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class VirtualFileSystem
    {

        public void Initialize(params IVirtualFileSource[] initialSources)
        {
            if (initialSources != null && initialSources.Length > 0)
            {
                foreach (var src in initialSources)
                    Mount(src);

                return;
            }

            // default behaviour (editor / development)

            Mount(new DirectorySource(Engine.Instance.ProjectSystem.EngineCachePath));
            Mount(new DirectorySource(Engine.Instance.ProjectSystem.AssetPath));
        }

        private readonly List<IVirtualFileSource> sources = [];

        public void Mount(IVirtualFileSource source)
        {
            sources.Insert(0, source);
        }

        public bool Exists(string path)
        {
            path = Normalize(path);

            foreach (var source in sources)
            {
                if (source.Exists(path))
                    return true;
            }

            return false;
        }

        public Stream OpenRead(string path)
        {
            path = Normalize(path);

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

        private static string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
