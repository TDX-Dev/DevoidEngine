using DevoidEngine.Serialization;
using DevoidGPU;
using System.Text.Json;

namespace DevoidEngine.Core
{
    public sealed class Shader : IDisposable
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

        public void Dispose()
        {
            foreach (var value in passes)
                value.Value.Dispose();


            passes.Clear();
        }

        public static Shader FromDescriptorFile(IGraphicsDevice device, string path, IReadOnlyDictionary<string, string>? defines = null)
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

            string baseDirectory = Path.GetDirectoryName(path)!;

            foreach (var passDesc in descriptor.Passes)
            {
                string? vertexPath = null;
                string? fragmentPath = null;
                string? computePath = null;

                if (!string.IsNullOrWhiteSpace(passDesc.Shaders.VS))
                {
                    vertexPath = Path.Combine(
                        baseDirectory,
                        passDesc.Shaders.VS);
                }

                if (!string.IsNullOrWhiteSpace(passDesc.Shaders.FS))
                {
                    fragmentPath = Path.Combine(
                        baseDirectory,
                        passDesc.Shaders.FS);
                }

                if (!string.IsNullOrWhiteSpace(passDesc.Shaders.CS))
                {
                    computePath = Path.Combine(
                        baseDirectory,
                        passDesc.Shaders.CS);
                }

                ShaderPass pass = new(device, vertexPath, fragmentPath, computePath);
                

                if (passDesc.States != null)
                {
                    pass.Blend =
                        ParseBlend(passDesc.States.Blend);

                    pass.Depth =
                        ParseDepth(passDesc.States.Depth);

                    pass.Rasterizer =
                        ParseRasterizer(
                            passDesc.States.Cull,
                            passDesc.States.Scissor);
                }

                shader.passes[passDesc.Name] = pass;

                if (passDesc.Name == descriptor.DefaultPass)
                    shader.DefaultPass = pass;
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
                ShaderResourceInfo? binding =
                    reflectionData.Resources.FirstOrDefault(x => x.Name == resource.Name);

                if (binding == null)
                    continue;

                switch (binding.Type)
                {
                    case ShaderResourceType.Texture2D:
                    case ShaderResourceType.TextureCube:
                    case ShaderResourceType.Texture2DArray:
                    case ShaderResourceType.Texture3D:
                    case ShaderResourceType.RWTexture2D:
                    case ShaderResourceType.RWTexture2DArray:
                    case ShaderResourceType.RWTexture3D:

                        layout.Textures[binding.Name] = binding;
                        break;
                }
            }

            foreach (var resource in reflectionData.Resources)
            {
                if (resource.Type == ShaderResourceType.Sampler)
                {
                    layout.Samplers[resource.Name] = resource;
                }
            }

            if (layout.Variables.Count == 0 &&
                (layout.Textures.Count == 0 && layout.Samplers.Count == 0))
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
            }

            foreach (var resource in reflection.Resources)
            {
                DescriptorType type = resource.Type switch
                {
                    ShaderResourceType.Texture2D or
                    ShaderResourceType.TextureCube or
                    ShaderResourceType.Texture2DArray or
                    ShaderResourceType.Texture3D
                        => DescriptorType.Texture,

                    ShaderResourceType.Sampler
                        => DescriptorType.Sampler,

                    ShaderResourceType.StructuredBuffer
                        => DescriptorType.StorageBuffer,

                    ShaderResourceType.RWStructuredBuffer
                        => DescriptorType.RWStorageBuffer,

                    ShaderResourceType.RWTexture2D or
                    ShaderResourceType.RWTexture2DArray or
                    ShaderResourceType.RWTexture3D
                        => DescriptorType.RWTexture,

                    _ => throw new InvalidOperationException()
                };

                bindings.Add(new DescriptorBinding
                {
                    Binding = (uint)resource.BindSlot,
                    Type = type,
                    Stages = resource.Stage
                });
            }



            return device.CreateDescriptorLayout(
                [.. bindings]);
        }

        public ShaderVariant GetVariant(IReadOnlyDictionary<string, string>? defines = null)
        {
            ShaderVariant variant = DefaultPass.GetVariant(defines);

            EnsureMaterialLayout(variant.ReflectionData);

            return variant;
        }

        private void EnsureMaterialLayout(ShaderReflectionData reflection)
        {
            if (MaterialLayout == null)
            {
                MaterialLayout =
                    BuildMaterialLayout(
                        ShaderDescriptor,
                        reflection);

                return;
            }

            ValidateMaterialLayout(reflection);
        }

        private void ValidateMaterialLayout(ShaderReflectionData reflection)
        {
            MaterialLayout? actual =
                BuildMaterialLayout(
                    ShaderDescriptor,
                    reflection);

            if (actual == null)
            {
                if (MaterialLayout != null)
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant does not contain the expected material layout.");
                }

                return;
            }

            if (MaterialLayout!.BufferName != actual.BufferName ||
                MaterialLayout.BufferSize != actual.BufferSize ||
                MaterialLayout.BufferBindSlot != actual.BufferBindSlot)
            {
                throw new InvalidOperationException(
                    $"Shader '{Name}' variant changed the material buffer layout.");
            }

            if (MaterialLayout.Variables.Count !=
                actual.Variables.Count)
            {
                throw new InvalidOperationException(
                    $"Shader '{Name}' variant changed the material variable layout.");
            }

            foreach (var expected in MaterialLayout.Variables)
            {
                if (!actual.Variables.TryGetValue(
                        expected.Key,
                        out ShaderVariableInfo? actualVariable))
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant removed material variable '{expected.Key}'.");
                }

                ShaderVariableInfo expectedVariable =
                    expected.Value;

                if (expectedVariable.Offset != actualVariable.Offset ||
                    expectedVariable.Size != actualVariable.Size)
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant changed the layout of material variable '{expected.Key}'.");
                }
            }

            if (MaterialLayout.Textures.Count !=
                actual.Textures.Count)
            {
                throw new InvalidOperationException(
                    $"Shader '{Name}' variant changed material texture bindings.");
            }

            foreach (var expected in MaterialLayout.Textures)
            {
                if (!actual.Textures.TryGetValue(
                        expected.Key,
                        out ShaderResourceInfo? actualResource))
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant removed material texture '{expected.Key}'.");
                }

                ShaderResourceInfo expectedResource =
                    expected.Value;

                if (expectedResource.BindSlot != actualResource.BindSlot ||
                    expectedResource.Type != actualResource.Type)
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant changed material texture '{expected.Key}'.");
                }
            }

            if (MaterialLayout.Samplers.Count !=
                actual.Samplers.Count)
            {
                throw new InvalidOperationException(
                    $"Shader '{Name}' variant changed material sampler bindings.");
            }

            foreach (var expected in MaterialLayout.Samplers)
            {
                if (!actual.Samplers.TryGetValue(
                        expected.Key,
                        out ShaderResourceInfo? actualResource))
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant removed material sampler '{expected.Key}'.");
                }

                ShaderResourceInfo expectedResource =
                    expected.Value;

                if (expectedResource.BindSlot != actualResource.BindSlot ||
                    expectedResource.Type != actualResource.Type)
                {
                    throw new InvalidOperationException(
                        $"Shader '{Name}' variant changed material sampler '{expected.Key}'.");
                }
            }
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

        private static RasterizerState ParseRasterizer(string? cull, string? scissor)
        {
            cull ??= "Back";

            RasterizerState rs = new()
            {
                CullMode = cull switch
                {
                    "Back" => CullMode.Back,
                    "Front" => CullMode.Front,
                    "None" => CullMode.None,
                    _ => throw new Exception($"Unknown cull mode '{cull}'")
                },

                EnableScissor = scissor switch
                {
                    "true" => true,
                    "false" => false,
                    null => false,
                    _ => throw new Exception($"Unknown scissor value '{scissor}'")
                }
            };

            return rs;
        }
    }
}
