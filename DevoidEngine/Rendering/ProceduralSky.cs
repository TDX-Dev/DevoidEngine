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

        public MaterialInstance Material { get; } = null!;

        public Mesh Mesh => PrimitiveMeshes.GetUVSphere();

        public ProceduralSky()
        {
            Shader proceduralSkyShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/sky_procedural.dsd");
            Material mat = new(proceduralSkyShader);

            Material = new MaterialInstance(mat);
        }
    }
}
