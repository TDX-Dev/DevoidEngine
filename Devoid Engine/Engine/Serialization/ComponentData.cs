using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    [MessagePackObject]
    public class ComponentData
    {
        [Key(0)] public string Type = "Component";

        [Key(1)] public byte[] Data = [];
    }
}
