using DevoidEngine.Serialization;
using DevoidGPU;
using System.Text.Json;

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
                ShaderStage? vertex = null;
                ShaderStage? fragment = null;
                ShaderStage? compute = null;

                if (!string.IsNullOrWhiteSpace(passDesc.Shaders.VS))
                {
                    string vsPath = Path.Combine(baseDirectory, passDesc.Shaders.VS);

                    vertex = new ShaderStage(
                        device.CreateShader(new ShaderDescription
                        {
                            Name = $"{descriptor.Name}_{passDesc.Name}_VS",
                            FilePath = vsPath,
                            Source = File.ReadAllText(vsPath),
                            EntryPoint = "VSMain",
                            Stage = DevoidGPU.ShaderStage.Vertex,
                            Defines = []
                        }));
                }

                if (!string.IsNullOrWhiteSpace(passDesc.Shaders.FS))
                {
                    string fsPath = Path.Combine(baseDirectory, passDesc.Shaders.FS);

                    fragment = new ShaderStage(
                        device.CreateShader(new ShaderDescription
                        {
                            Name = $"{descriptor.Name}_{passDesc.Name}_FS",
                            FilePath = fsPath,
                            Source = File.ReadAllText(fsPath),
                            EntryPoint = "PSMain",
                            Stage = DevoidGPU.ShaderStage.Fragment,
                            Defines = []
                        }));
                }

                if (!string.IsNullOrWhiteSpace(passDesc.Shaders.CS))
                {
                    string csPath = Path.Combine(baseDirectory, passDesc.Shaders.CS);

                    compute = new ShaderStage(
                        device.CreateShader(new ShaderDescription
                        {
                            Name = $"{descriptor.Name}_{passDesc.Name}_CS",
                            FilePath = csPath,
                            Source = File.ReadAllText(csPath),
                            EntryPoint = "CSMain",
                            Stage = DevoidGPU.ShaderStage.Compute,
                            Defines = []
                        }));
                }

                ShaderPass pass = new(vertex, fragment, compute);

                List<ShaderReflectionData> reflections = [];

                if (vertex != null)
                    reflections.Add(vertex.ShaderReflectionData);

                if (fragment != null)
                    reflections.Add(fragment.ShaderReflectionData);

                if (compute != null)
                    reflections.Add(compute.ShaderReflectionData);

                ShaderReflectionData reflection_data =
                    ShaderReflectionData.Merge([.. reflections]);

                ShaderReflectionData.Print(reflection_data);

                shader.MaterialLayout ??= BuildMaterialLayout(descriptor, reflection_data);

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

                if (pass.Vertex != null && pass.Fragment != null)
                {
                    pass.Pipeline =
                        device.CreateGraphicsPipeline(
                            new GraphicsPipelineDescription
                            {
                                VertexShader = pass.Vertex!.GPU,
                                PixelShader = pass.Fragment!.GPU,

                                PipelineLayout = pass.PipelineLayout,

                                Topology = PrimitiveType.Triangles,

                                VertexLayout = Vertex.VertexInfo, // HARDCODED FOR THE TIME BEING, ISSUE THO

                                Rasterizer = pass.Rasterizer,
                                DepthStencil = pass.Depth,
                                Blend = pass.Blend
                            });
                }

                if (pass.Compute != null)
                {
                    pass.ComputePipeline =
                        device.CreateComputePipeline(new ComputePipelineDescription()
                        {
                            ComputeShader = pass.Compute.GPU
                        });
                }

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
            //ShaderPass pass,
            ShaderDescriptor descriptor,
            ShaderReflectionData reflectionData
        )
        {
            MaterialLayout layout = new();

            if (descriptor.MaterialParameters != null)
            {
                UniformBufferInfo? materialBuffer =
                    reflectionData.UniformBuffers.FirstOrDefault(x =>
                        string.Equals(
                            x.Name,
                            descriptor.MaterialParameters.BufferName,
                            StringComparison.OrdinalIgnoreCase));

                if (materialBuffer != null)
                {
                    layout.BufferName = materialBuffer.Name;
                    layout.BufferSize = materialBuffer.Size;
                    layout.BufferBindSlot = materialBuffer.BindSlot;

                    foreach (var variable in materialBuffer.Variables)
                        layout.Variables[variable.Name] = variable;
                }
            }

            foreach (var resource in descriptor.Resources)
            {
                switch (resource.Kind)
                {
                    case ShaderResourceKind.SampledTexture:
                        {
                            TextureBindingInfo? binding =
                                reflectionData.TextureBindings
                                    .FirstOrDefault(x => x.Name == resource.Name);

                            if (binding == null)
                            {
                                Console.WriteLine(
                                    $"Material texture '{resource.Name}' not found.");
                                continue;
                            }

                            layout.Textures[binding.Name] = binding;
                            break;
                        }

                    case ShaderResourceKind.StorageTexture:
                        {
                            StorageTextureBindingInfo? binding =
                                reflectionData.StorageTextureBindings
                                    .FirstOrDefault(x => x.Name == resource.Name);

                            if (binding == null)
                            {
                                Console.WriteLine(
                                    $"Storage texture '{resource.Name}' not found.");
                                continue;
                            }

                            layout.StorageTextures[binding.Name] = binding;
                            break;
                        }

                    case ShaderResourceKind.StorageBuffer:
                        {
                            StorageBufferBindingInfo? binding =
                                reflectionData.StorageBufferBindings
                                    .FirstOrDefault(x => x.Name == resource.Name);

                            if (binding == null)
                                continue;

                            layout.StorageBuffers[binding.Name] = binding;
                            break;
                        }
                }
            }

            foreach (var samplerBinding in reflectionData.SamplerBindings)
            {
                layout.Samplers[samplerBinding.Name] = samplerBinding;
            }

            if (layout.Variables.Count == 0 &&
                layout.Textures.Count == 0 &&
                layout.Samplers.Count == 0
            )
            {
                return null;
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

            foreach (var texture in reflection.StorageTextureBindings)
            {
                bindings.Add(new DescriptorBinding
                {
                    Binding = (uint)texture.BindSlot,
                    Type = DescriptorType.RWTexture,
                    Stages = texture.Stage
                });
            }

            foreach (var buffer in reflection.StorageBufferBindings)
            {
                bindings.Add(new DescriptorBinding
                {
                    Binding = (uint)buffer.BindSlot,
                    Type = DescriptorType.RWStorageBuffer,
                    Stages = buffer.Stage
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
