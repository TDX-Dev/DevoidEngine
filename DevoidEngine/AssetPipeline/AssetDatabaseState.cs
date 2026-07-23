using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline
{
    [MessagePackObject]
    public class AssetDatabaseState
    {
        [Key(0)]
        public Dictionary<Guid, AssetEntry> Entries = [];
    }
}
