using DevoidEngine.Core;

namespace DevoidEngine.Rendering
{
    public struct RenderView
    {
        public Camera Camera = null!;

        public readonly List<RenderMeshData> Objects;

        public GPUPointLight[] PointLights;
        public int PointLightCount;

        public GPUSpotLight[] SpotLights;
        public int SpotLightCount;

        public GPUDirectionalLight[] DirectionalLights;
        public int DirectionalLightCount;

        public RenderView()
        {
            Objects = [];
            PointLights = new GPUPointLight[Renderer.MAX_POINT_LIGHTS];
            SpotLights = new GPUSpotLight[Renderer.MAX_SPOT_LIGHTS];
            DirectionalLights = new GPUDirectionalLight[Renderer.MAX_DIRECTIONAL_LIGHTS];
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
