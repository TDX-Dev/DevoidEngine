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

        public readonly string OutputFinalName => $"{Guid:N}.{OutputExtension}";
        public readonly string OutputFinalPath => Path.Join(OutputDirectory, OutputFinalName);

        public readonly string GetOutputPath(
            ulong localId,
            string extension
        )
        {
            return Path.Combine(
                OutputDirectory,
                $"{Guid:N}-{localId}.{extension}");
        }

        public readonly string GetRootOutputPath(
            string extension
        )
        {
            return Path.Combine(
                OutputDirectory,
                $"{Guid:N}.{extension}");
        }
    }
}
