using DevoidEngine.Engine.Core;
using System.Numerics;

namespace DevoidEngine.Engine.Rendering
{
    public static class RenderingDefaults
    {
        public static Material DefaultMaterial = null!;

        public static void Initialize()
        {
            var shader = ShaderLibrary.GetShader("PBR/ForwardPBR");
            if (shader == null)
                throw new Exception("Forward PBR shader not initialized");
            DefaultMaterial = new Material(shader);
            DefaultMaterial.SetVector4("Albedo", new Vector4(1, 1, 1, 1));
            DefaultMaterial.SetVector3("EmissiveColor", new Vector3(1, 1, 1));
            DefaultMaterial.SetFloat("EmissiveStrength", 1);
            //DefaultMaterial.SetFloat("Roughness", 1f);
            //DefaultMaterial.SetFloat("Metallic", 0f);
            DefaultMaterial.SetFloat("AO", 1);
            Renderer.SkyboxRenderer.BindIBL(DefaultMaterial);
        }

        public static MaterialInstance GetMaterial() => new MaterialInstance(DefaultMaterial);




    }
}