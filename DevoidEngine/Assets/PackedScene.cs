namespace DevoidEngine.Assets
{
    public class PackedScene
    {
        public PackedSceneNode[] Nodes = [];

        public Guid[] MeshGuids = [];
        public Guid[] MaterialGuids = [];

        public PackedCamera[] Cameras = [];
        public PackedLight[] Lights = [];
    }
}
