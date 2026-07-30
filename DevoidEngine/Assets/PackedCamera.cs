using DevoidEngine.Core;
using MessagePack;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class PackedCamera
    {
        [Key(0)]
        public string Name = "";
        [Key(1)]
        public ProjectionType Projection;
        [Key(2)]
        public float FOV;
        [Key(3)]
        public float Near;
        [Key(4)]
        public float Far;
        [Key(5)]
        public float Aspect;
        [Key(6)]
        public float OrthoWidth;
    }
}
