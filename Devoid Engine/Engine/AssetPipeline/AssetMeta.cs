using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    public class AssetMeta
    {
        public string Guid;
        public string Importer;
        public int Version = 1;

        public byte[] Settings;

        public long SourceTimestamp;
    }
}
