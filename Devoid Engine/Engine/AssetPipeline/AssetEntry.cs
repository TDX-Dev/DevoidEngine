using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    [MessagePackObject]
    public class AssetEntry
    {
        [Key(0)]
        public required Guid Guid;
        [Key(1)]
        public required string AssetPath;
        [Key(2)]
        public required string MetaPath;

        [Key(3)]
        public Guid? ContainerGuid = null;
        [Key(4)]
        public ulong LocalId = 0;
    }
}
