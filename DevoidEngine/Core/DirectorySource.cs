namespace DevoidEngine.Core
{
    public class DirectorySource : IVirtualFileSource
    {
        private readonly string _root;

        public DirectorySource(string root)
        {
            this._root = root;
        }

        public bool Exists(string path)
        {
            return File.Exists(Path.Combine(_root, path));
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(Path.Combine(_root, path));
        }

        public IEnumerable<string> Enumerate(string path)
        {
            var dir = Path.Combine(_root, path);

            if (!Directory.Exists(dir))
                return [];

            return Directory.GetFiles(dir);
        }
    }
}
