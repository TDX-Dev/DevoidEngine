using SharpDX.D3DCompiler;

namespace DevoidGPU.DX11
{
    class Dx11ShaderIncludeHandler : Include
    {
        private readonly Dictionary<Stream, string> _streamPaths = [];
        private readonly string _rootDirectory;
        public IDisposable? Shadow { get; set; }

        // Track opened streams to dispose safely
        private readonly List<Stream> _openedStreams = [];

        public Dx11ShaderIncludeHandler(string root)
        {
            _rootDirectory = root;
        }

        public Stream Open(
    IncludeType type,
    string fileName,
    Stream parentStream)
        {
            string baseDir;

            if (parentStream != null &&
                _streamPaths.TryGetValue(parentStream, out string? parentPath))
            {
                baseDir = Path.GetDirectoryName(parentPath)!;
            }
            else
            {
                baseDir = _rootDirectory;
            }

            string fullPath =
                Path.GetFullPath(
                    Path.Combine(baseDir, fileName));

            if (!File.Exists(fullPath))
                throw new FileNotFoundException(
                    $"Shader include not found: {fullPath}");

            string text = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
            byte[] cleanBytes = System.Text.Encoding.UTF8.GetBytes(text);

            var stream = new MemoryStream(cleanBytes);

            _streamPaths[stream] = fullPath;
            _openedStreams.Add(stream);

            return stream;
        }

        //public Stream Open(IncludeType type, string fileName, Stream parentStream)
        //{

        //    string baseDirectory = rootDirectory;

        //    string fullPath = Path.GetFullPath(Path.Combine(baseDirectory, fileName));

        //    if (!File.Exists(fullPath))
        //        fullPath = Path.GetFullPath(Path.Combine(rootDirectory, fileName));

        //    if (!File.Exists(fullPath))
        //        throw new FileNotFoundException($"Shader include not found: {fullPath}");

        //    string text = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
        //    byte[] cleanBytes = System.Text.Encoding.UTF8.GetBytes(text);

        //    var stream = new MemoryStream(cleanBytes);

        //    openedStreams.Add(stream);

        //    return stream;
        //}
        public void Close(Stream stream)
        {
            stream?.Dispose();
        }

        public void Dispose()
        {
            foreach (var stream in _openedStreams)
                stream.Dispose();

            _openedStreams.Clear();
        }
    }
}