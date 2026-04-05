using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    [MessagePackObject]
    public class GameObjectData
    {
        [Key(0)]
        public Guid Id;

        [Key(1)]
        public string Name = "";

        [Key(2)]
        public Guid Parent;

        [Key(3)]
        public Vector3 Position;
        [Key(4)]
        public Quaternion Rotation;
        [Key(5)]
        public Vector3 Scale;

        [Key(6)]
        public List<ComponentData> Components = new();
    }
}
