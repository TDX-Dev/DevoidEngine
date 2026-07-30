using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public struct PackedMesh
    {
        [Key(0)]
        public int MeshIndex;

        [Key(1)]
        public int MaterialIndex;
    }
}
