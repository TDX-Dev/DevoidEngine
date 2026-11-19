using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public struct RenderView
    {
        public Camera Camera = null!;

        public readonly List<RenderMeshData> Objects;

        public GpuPointLight[] PointLights;
        public int PointLightCount;

        public GpuSpotLight[] SpotLights;
        public int SpotLightCount;

        public GpuDirectionalLight[] DirectionalLights;
        public int DirectionalLightCount;

        public RenderView()
        {
            Objects = [];
            PointLights = new GpuPointLight[Renderer.MAX_POINT_LIGHTS];
            SpotLights = new GpuSpotLight[Renderer.MAX_SPOT_LIGHTS];
            DirectionalLights = new GpuDirectionalLight[Renderer.MAX_DIRECTIONAL_LIGHTS];
        }

        public void Clear()
        {
            Objects.Clear();
            PointLightCount = 0;
            SpotLightCount = 0;
            DirectionalLightCount = 0;
        }
    }
}
