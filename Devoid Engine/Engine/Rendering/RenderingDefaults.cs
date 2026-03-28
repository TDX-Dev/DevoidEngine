using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    public static class RenderingDefaults
    {
        public static Material DefaultMaterial;

        public static void Initialize()
        {
            DefaultMaterial = new Material(ShaderLibrary.GetShader("PBR/ForwardPBR"));
            DefaultMaterial.SetVector4("Albedo", new System.Numerics.Vector4(1, 1, 1, 1));
            //DefaultMaterial.SetFloat("Roughness", 1f);
            //DefaultMaterial.SetFloat("Metallic", 0f);
            DefaultMaterial.SetFloat("AO", 1);
            Renderer.SkyboxRenderer.BindIBL(DefaultMaterial);
        }

        public static MaterialInstance GetMaterial() => new MaterialInstance(DefaultMaterial);




    }
}