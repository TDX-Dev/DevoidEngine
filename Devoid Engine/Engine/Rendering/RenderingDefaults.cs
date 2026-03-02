using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public static class RenderingDefaults
    {
        public static Material DefaultMaterial;

        public static void Initialize()
        {
            DefaultMaterial = new Material(ShaderLibrary.GetShader("PBR/ForwardPBR"));
            DefaultMaterial.SetVector4("Albedo", new System.Numerics.Vector4(1, 1, 1, 1));
            DefaultMaterial.SetFloat("AO", 1);
            DefaultMaterial.SetVector3("SpecularColor", new System.Numerics.Vector3(0.04f));
        }

        public static MaterialInstance GetMaterial() => new MaterialInstance(DefaultMaterial);




    }
}
