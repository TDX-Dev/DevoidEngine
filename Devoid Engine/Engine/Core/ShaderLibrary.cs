namespace DevoidEngine.Engine.Core
{
    public static class ShaderLibrary
    {
        static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();


        static ShaderLibrary()
        {
            shaders["BASIC_SHADER"] = new Shader("Engine/Content/Shaders/Testing/basic");
            shaders["PBR/ClusteredPBR"] = new Shader("Engine/Content/Shaders/PBR/clustered_pbr");
            shaders["PBR/ForwardPBR"] = new Shader("Engine/Content/Shaders/PBR/pbr_forward");
            shaders["Screen/RENDER_SCREEN"] = new Shader("Engine/Content/Shaders/Screen/renderToScreen");
        }

        public static void RegisterShader(string name, Shader shader)
        {
            if (shaders.ContainsKey(name))
            {
                return;
            }
            else
            {
                shaders.Add(name, shader);
            }
        }

        public static Shader GetShader(string name)
        {
            if (shaders.TryGetValue(name, out Shader shader))
                return shader;
            else
                return null;
        }


    }
}