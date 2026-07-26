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
        public MaterialLayout? MaterialLayout { get; private set; } = null!;
        public ShaderDescriptor ShaderDescriptor { get; private set; } = null!;
        public ShaderPass DefaultPass { get; private set; } = null!;

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

                ShaderReflectionData reflection_data = ShaderReflectionData.Merge(pass.Vertex.ShaderReflectionData, pass.Fragment.ShaderReflectionData);

                shader.MaterialLayout ??= BuildMaterialLayout( pass, descriptor.MaterialParameters, reflection_data);

                pass.DescriptorLayout = CreateDescriptorLayout(device, reflection_data);

                if (passDesc.States != null)
                {
                    pass.Blend =
                        ParseBlend(passDesc.States.Blend);

                    pass.Depth =
                        ParseDepth(passDesc.States.Depth);

                    pass.Rasterizer =
                        ParseRasterizer(passDesc.States.Cull);
                }

                pass.Pipeline =
                    device.CreateGraphicsPipeline(
                        new GraphicsPipelineDescription
                        {
                            VertexShader = pass.Vertex.GPU,
                            PixelShader = pass.Fragment.GPU,

                            PipelineLayout = pass.PipelineLayout,

                            Topology = PrimitiveType.Triangles,

                            VertexLayout = Vertex.VertexInfo, // HARDCODED FOR THE TIME BEING, ISSUE THO

                            Rasterizer = pass.Rasterizer,
                            DepthStencil = pass.Depth,
                            Blend = pass.Blend
                        });

                shader.passes[passDesc.Name] = pass;

                if (passDesc.Name == descriptor.DefaultPass)
                {
                    shader.DefaultPass = pass;
                }
            }

            shader.ShaderDescriptor = descriptor;
            return shader;
        }

        private static MaterialLayout? BuildMaterialLayout(
            ShaderPass pass,
            MaterialParameterDescriptor? materialDesc,
            ShaderReflectionData reflectionData
        )
        {
            if (materialDesc == null)
                return null;

            UniformBufferInfo? materialBuffer = null;

            foreach (var buffer in reflectionData.UniformBuffers)
            {
                if (string.Equals(
                        buffer.Name,
                        materialDesc.BufferName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    materialBuffer = buffer;
                    break;
                }
            }

            if (materialBuffer == null)
            {
                return null;
            }

            MaterialLayout layout = new()
            {
                BufferName = materialBuffer.Name,
                BufferSize = materialBuffer.Size,
                BufferBindSlot = materialBuffer.BindSlot
            };

            foreach (var variable in materialBuffer.Variables)
            {
                layout.Variables[variable.Name] = variable;
            }

            foreach (var textureDesc in materialDesc.Textures)
            {
                TextureBindingInfo? binding =
                    reflectionData.TextureBindings
                        .FirstOrDefault(x => x.Name == textureDesc.Name) ?? throw new Exception(
                        $"Material texture '{textureDesc.Name}' not found.");
                layout.Textures[binding.Name] = binding;
            }

            foreach (var samplerBinding in reflectionData.SamplerBindings)
            {
                layout.Samplers[samplerBinding.Name] = samplerBinding;
            }

            return layout;
        }

        private static IDescriptorLayout CreateDescriptorLayout(
            IGraphicsDevice device,
            ShaderReflectionData reflection
        )
        {
            List<DescriptorBinding> bindings = [];


            foreach (var buffer in reflection.UniformBuffers)
            {
                bindings.Add(new DescriptorBinding
                {
                    Binding = (uint)buffer.BindSlot,
                    Type = DescriptorType.UniformBuffer,
                    Stages = buffer.Stages
                });

                if (buffer.BindSlot == 3)
                {
                    Console.WriteLine(buffer.Stages);
                }
            }

            foreach (var texture in reflection.TextureBindings)
            {
                bindings.Add(new DescriptorBinding
                {
                    Binding = (uint)texture.BindSlot,
                    Type = DescriptorType.Texture,
                    Stages = texture.Stage
                });
            }

            foreach (var sampler in reflection.SamplerBindings)
            {
                bindings.Add(new DescriptorBinding
                {
                    Binding = (uint)sampler.BindSlot,
                    Type = DescriptorType.Sampler,
                    Stages = sampler.Stage
                });
            }

            return device.CreateDescriptorLayout(
                [.. bindings]);
        }

        private static BlendStateDescription ParseBlend(string? blend)
        {
            blend ??= "Off";

            BlendState state = blend switch
            {
                "Off" => new BlendState
                {
                    Enable = false,
                    WriteMask = ColorMask.All
                },

                "Alpha" => new BlendState
                {
                    Enable = true,

                    SrcColor = BlendFactor.SrcAlpha,
                    DstColor = BlendFactor.InvSrcAlpha,
                    ColorOp = BlendOp.Add,

                    SrcAlpha = BlendFactor.One,
                    DstAlpha = BlendFactor.InvSrcAlpha,
                    AlphaOp = BlendOp.Add,

                    WriteMask = ColorMask.All
                },

                "Additive" => new BlendState
                {
                    Enable = true,

                    SrcColor = BlendFactor.SrcAlpha,
                    DstColor = BlendFactor.One,
                    ColorOp = BlendOp.Add,

                    SrcAlpha = BlendFactor.One,
                    DstAlpha = BlendFactor.One,
                    AlphaOp = BlendOp.Add,

                    WriteMask = ColorMask.All
                },

                _ => throw new Exception($"Unknown blend mode '{blend}'")
            };

            return new BlendStateDescription
            {
                AlphaToCoverage = false,
                IndependentBlend = false,

                BlendStates =
                [
                    state
                ]
            };
        }

        private static DepthStencilState ParseDepth(string? depth)
        {
            depth ??= "LessEqual";

            return depth switch
            {
                "Less" => new DepthStencilState
                {
                    DepthTest = true,
                    DepthWrite = true,
                    DepthFunc = CompareFunc.Less
                },

                "LessEqual" => new DepthStencilState
                {
                    DepthTest = true,
                    DepthWrite = true,
                    DepthFunc = CompareFunc.LessEqual
                },

                "Always" => new DepthStencilState
                {
                    DepthTest = true,
                    DepthWrite = false,
                    DepthFunc = CompareFunc.Always
                },

                "Disabled" => new DepthStencilState
                {
                    DepthTest = false,
                    DepthWrite = false,
                    DepthFunc = CompareFunc.LessEqual
                },

                _ => throw new Exception($"Unknown depth mode '{depth}'")
            };
        }

        private static RasterizerState ParseRasterizer(string? cull)
        {
            cull ??= "Back";

            return cull switch
            {
                "Back" => new RasterizerState
                {
                    CullMode = CullMode.Back
                },

                "Front" => new RasterizerState
                {
                    CullMode = CullMode.Front
                },

                "None" => new RasterizerState
                {
                    CullMode = CullMode.None
                },

                _ => throw new Exception($"Unknown cull mode '{cull}'")
            };
        }
    }
}
