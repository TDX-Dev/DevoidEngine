using DevoidEngine.Serialization;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class Shader
    {
        private readonly Dictionary<string, ShaderPass> passes = [];

        public string Name { get; private set; } = string.Empty;

        public ShaderPass GetPass(string name)
        {
            return passes[name];
        }

        public bool HasPass(string name)
        {
            return passes.ContainsKey(name);
        }

        public static Shader FromDescriptorFile(
            IGraphicsDevice device,
            string path)
        {
            string json = File.ReadAllText(path);

            ShaderDescriptor descriptor =
                JsonSerializer.Deserialize(
                    json,
                    ShaderJsonContext.Default.ShaderDescriptor)!;

            Shader shader = new()
            {
                Name = descriptor.Name
            };

            string baseDirectory =
                Path.GetDirectoryName(path)!;

            foreach (var passDesc in descriptor.Passes)
            {
                string vsPath = Path.Combine(
                    baseDirectory,
                    passDesc.Shaders.VS);

                string fsPath = Path.Combine(
                    baseDirectory,
                    passDesc.Shaders.FS);

                ShaderStage vertex = new(
                    device.CreateShader(new ShaderDescription
                    {
                        Name = $"{descriptor.Name}_{passDesc.Name}_VS",
                        FilePath = vsPath,
                        Source = File.ReadAllText(vsPath),
                        EntryPoint = "VSMain",
                        Stage = DevoidGPU.ShaderStage.Vertex,
                        Defines = []
                    }));

                ShaderStage fragment = new(
                    device.CreateShader(new ShaderDescription
                    {
                        Name = $"{descriptor.Name}_{passDesc.Name}_FS",
                        FilePath = fsPath,
                        Source = File.ReadAllText(fsPath),
                        EntryPoint = "PSMain",
                        Stage = DevoidGPU.ShaderStage.Fragment,
                        Defines = []
                    }));

                ShaderPass pass = new(vertex, fragment);

                // -------------------------
                // STATES
                // -------------------------

                if (passDesc.States != null)
                {
                    pass.Blend =
                        ParseBlend(passDesc.States.Blend);

                    pass.Depth =
                        ParseDepth(passDesc.States.Depth);

                    pass.Rasterizer =
                        ParseRasterizer(passDesc.States.Cull);
                }

                shader.passes[passDesc.Name] = pass;
            }

            return shader;
        }

        private static BlendStateDescription ParseBlend(string? blend)
        {
            return new BlendStateDescription();
        }

        private static DepthStencilState ParseDepth(string? depth)
        {
            return new DepthStencilState();
        }

        private static RasterizerState ParseRasterizer(string? cull)
        {
            return new RasterizerState();
        }
    }
}
