using SharpDX.D3DCompiler;

namespace DevoidGPU.DX11
{
    class DX11ShaderIncludeHandler : Include
    {
        private readonly string rootDirectory;
        public IDisposable? Shadow { get; set; }

        // Track opened streams to dispose safely
        private readonly List<Stream> openedStreams = [];

        public DX11ShaderIncludeHandler(string root)
        {
            rootDirectory = root;
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {

            string baseDirectory = rootDirectory;

            string fullPath = Path.GetFullPath(Path.Combine(baseDirectory, fileName));

            if (!File.Exists(fullPath))
                fullPath = Path.GetFullPath(Path.Combine(rootDirectory, fileName));

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Shader include not found: {fullPath}");

            string text = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
            byte[] cleanBytes = System.Text.Encoding.UTF8.GetBytes(text);

            var stream = new MemoryStream(cleanBytes);

            openedStreams.Add(stream);

            return stream;
        }
        public void Close(Stream stream)
        {
            stream?.Dispose();
        }

        public void Dispose()
        {
            foreach (var stream in openedStreams)
                stream.Dispose();

            openedStreams.Clear();
        }
    }
}