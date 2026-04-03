using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public class AssetMeta
    {
        public required string Guid;
        public required string Importer;
        public required int Version = 1;

        public required byte[] Settings;

        public required long SourceTimestamp;
    }
}
