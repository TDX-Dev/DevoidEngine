using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serialization;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Assets
{
    [MessagePackObject]
    public class SceneAsset
    {
        [Key(0)]
        public SceneData Scene = new SceneData();
    }
}
