using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    internal class AssetEntry
    {
        public required Guid Guid;
        public required string AssetPath;
        public required string MetaPath;

        public Guid? ContainerGuid = Guid.Empty;
        public ulong LocalId = 0;
    }
}
