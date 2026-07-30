using DevoidEngine.Core;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public sealed class ProceduralSky : ISky
    {
        public Vector3 SunDirection;

        public float Turbidity;

        public float Exposure;

        public bool Dirty { get; private set; } = true;

        public MaterialInstance Material { get; } = null!;

        public Mesh Mesh => PrimitiveMeshes.GetCube();

        public void BuildEnvironment(RenderContext context)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
