using DevoidEngine.Rendering;
using MessagePack;
using System.Numerics;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class PackedLight
    {
        [Key(0)]
        public string Name = "";
        [Key(1)]
        public LightType Type;
        [Key(2)]
        public Vector3 Color;
        [Key(3)]
        public float Intensity;
        [Key(4)]
        public float Range;
        [Key(5)]
        public float InnerCone;
        [Key(6)]
        public float OuterCone;
    }
}
