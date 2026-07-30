using DevoidEngine.Rendering;
using System.Numerics;

namespace DevoidEngine.Assets
{
    public class PackedLight
    {
        public string Name = "";

        public LightType Type;

        public Vector3 Color;

        public float Intensity;

        public float Range;

        public float InnerCone;

        public float OuterCone;
    }
}
