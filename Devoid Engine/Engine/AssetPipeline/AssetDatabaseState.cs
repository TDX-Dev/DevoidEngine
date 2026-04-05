using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline
{
    class AssetDatabaseState
    {
        public Dictionary<Guid, AssetEntry> Entries = new();
    }
}
