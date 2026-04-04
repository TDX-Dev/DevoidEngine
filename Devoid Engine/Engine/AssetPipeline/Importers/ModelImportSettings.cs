using DevoidEngine.Engine.Utilities;
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
        public Axis SourceUp = Axis.Y;

        [Key(1)]
        public Axis SourceForward = Axis.NegZ;

        [Key(2)]
        public float ImportScale = 1.0f;

        [Key(3)]
        public bool FlipUVs = false;
    }
}
