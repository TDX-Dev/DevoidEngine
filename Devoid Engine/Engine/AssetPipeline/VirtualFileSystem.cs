using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public class VirtualFileSystem
    {
        public static VirtualFileSystem Instance { get; private set; }
        
        public static void Initialize()
        {
            Instance = new VirtualFileSystem();

            var project = ProjectManager.Current;

            Instance.Mount(new DirectorySource(project.LibraryPath));
            Instance.Mount(new DirectorySource(project.AssetPath));
        }

        private readonly List<IVirtualFileSource> sources = new();

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
