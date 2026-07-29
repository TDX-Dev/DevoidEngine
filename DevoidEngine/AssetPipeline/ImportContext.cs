using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline
{
    public struct ImportContext
    {
        public string AssetPath;
        public Guid Guid;
        public string OutputDirectory;

        public string OutputExtension;

        public string OutputFinalName => $"{Guid:N}.{OutputExtension}";
        public string OutputFinalPath => Path.Join(OutputDirectory, OutputFinalName);
    }
}
