using DevoidEngine.Engine.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class Model : AssetType
    {
        public Mesh[] Meshes = [];
        public ModelNode[] Nodes = [];
    }
}
