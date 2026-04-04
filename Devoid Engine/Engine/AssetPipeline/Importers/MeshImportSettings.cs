using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    [MessagePackObject]
    public class ModelImportSettings
    {
        [Key(0)]
        public bool GenerateNormals = true;

        [Key(1)]
        public bool FlipUVs = false;
    }
}
