using MessagePack;
using System.Numerics;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class PackedSceneNode
    {
        [Key(0)]
        public string Name = "";
        [Key(1)]
        public int Parent = -1;
        [Key(2)]
        public PackedMesh[] Meshes = [];
        [Key(3)]
        public int Camera = -1;
        [Key(4)]
        public int Light = -1;
        [Key(5)]
        public Vector3 Translation;
        [Key(6)]
        public Quaternion Rotation;
        [Key(7)]
        public Vector3 Scale;
    }
}
