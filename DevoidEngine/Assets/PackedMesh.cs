using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class PackedMesh
    {
        [Key(0)]
        public int MeshIndex;

        [Key(1)]
        public Guid[] MaterialGuids = [];
    }
}
