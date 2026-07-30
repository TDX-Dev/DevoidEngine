using MessagePack;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class PackedScene
    {
        [Key(0)]
        public PackedSceneNode[] Nodes = [];
        [Key(1)]
        public Guid[] MeshGuids = [];
        [Key(2)]
        public Guid[] MaterialGuids = [];
        [Key(3)]
        public PackedCamera[] Cameras = [];
        [Key(4)]
        public PackedLight[] Lights = [];
    }
}
