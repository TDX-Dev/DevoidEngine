using System.Numerics;

namespace DevoidEngine.Assets
{
    public class PackedSceneNode
    {
        public string Name = "";

        public int Parent = -1;

        public int[] MeshIndices = [];

        public int Camera = -1;

        public int Light = -1;

        public Vector3 Translation;

        public Quaternion Rotation;

        public Vector3 Scale;
    }
}
