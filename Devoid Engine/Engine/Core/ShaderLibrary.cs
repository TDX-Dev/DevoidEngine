﻿using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public static class ShaderLibrary
    {
        static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();


        static ShaderLibrary()
        {
            shaders["BASIC_SHADER"] = new Shader("Engine/Content/Shaders/Testing/basic");
            shaders["PBR/ClusteredPBR"] = new Shader("Engine/Content/Shaders/PBR/clustered_pbr");
        }

        public static void RegisterShader(string name, Shader shader)
        {
            if (shaders.ContainsKey(name))
            {
                return;
            } else
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
