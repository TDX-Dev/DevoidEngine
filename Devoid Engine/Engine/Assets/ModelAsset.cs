using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Assets
{
    [MessagePackObject]
    public class ModelAsset
    {
        [Key(0)]
        public ModelNode[] Nodes = [];

        [Key(1)]
        public MeshAsset[] Meshes = [];

        [Key(2)]
        public Guid[] Materials = [];
    }
}
