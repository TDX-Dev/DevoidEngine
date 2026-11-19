using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11Shader : IShader
    {
        public ShaderDescription Description { get; }
        public ShaderStage Stage { get; }
        public ShaderReflectionData ReflectionData { get; private set; } = null!;
        public string Name { get; }

        internal byte[] Bytecode = null!;

        internal VertexShader? Vs;
        internal PixelShader? Ps;
        internal GeometryShader? Gs;
        internal ComputeShader? Cs;

        private readonly Device _device;

        public Dx11Shader(Device dx11Device, ShaderDescription desc)
        {
            _device = dx11Device;
            Stage = desc.Stage;
            Name = desc.Name ?? desc.Stage.ToString();
            Description = desc;

            Compile(desc);
        }

        public void Compile(ShaderDescription description)
        {
            string source = description.Source;
            string entryPoint = description.EntryPoint;
            string path = description.FilePath;

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Shader source cannot be empty.", nameof(description));

            if (string.IsNullOrWhiteSpace(entryPoint))
                throw new ArgumentException("Entry point cannot be empty.", nameof(description));

            string profile = GetProfileForType(Stage);

            ShaderMacro[] shaderDefines = new ShaderMacro[description.Defines.Count];

            int x = 0;
            foreach (KeyValuePair<string, string> kvp in description.Defines)
                shaderDefines[x++] = new ShaderMacro(kvp.Key, kvp.Value);


            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#endif
            CompilationResult result;
            result = ShaderBytecode.Compile(source, entryPoint, profile, flags, EffectFlags.None, shaderDefines, new Dx11ShaderIncludeHandler(Path.GetDirectoryName(path)!), Path.GetFileName(path)!);
            if (result.HasErrors)
                throw new Exception($"Shader compile error ({Name}): {result.Message}");

            Bytecode = result.Bytecode;

            CreateReflectionInfoFromBytecode();
            CreateShaderFromBytecode();
        }

        private void CreateShaderFromBytecode()
        {
            switch (Stage)
            {
                case ShaderStage.Vertex:
                    Vs = new VertexShader(_device, Bytecode);
                    break;
                case ShaderStage.Fragment:
                    Ps = new PixelShader(_device, Bytecode);
                    break;
                case ShaderStage.Geometry:
                    Gs = new GeometryShader(_device, Bytecode);
                    break;
                case ShaderStage.Compute:
                    Cs = new ComputeShader(_device, Bytecode);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported shader type: {Stage}");
            }
        }


        private void CreateReflectionInfoFromBytecode()
        {
            ReflectionData = new ShaderReflectionData();

            ShaderReflection reflection = new(Bytecode);
            SharpDX.D3DCompiler.ShaderDescription desc = reflection.Description;

            // Fill in constant buffers
            for (int i = 0; i < desc.ConstantBuffers; i++)
            {
                ConstantBuffer cb = reflection.GetConstantBuffer(i);
                ConstantBufferDescription cbDesc = cb.Description;

                UniformBufferInfo bufferInfo = new()
                {
                    Name = cbDesc.Name,
                    Size = cbDesc.Size,
                    BindSlot = GetBindSlot(reflection, cbDesc.Name),
                    Stages = Stage
                };

                for (int v = 0; v < cbDesc.VariableCount; v++)
                {
                    var variable = cb.GetVariable(v);
                    var varDesc = variable.Description;
                    var varType = variable.GetVariableType();
                    var typeDesc = varType.Description;
                    var svt = Dx11StateMapper.ConvertResourceType(typeDesc.Name);

                    bufferInfo.Variables.Add(new ShaderVariableInfo
                    {
                        Name = varDesc.Name,
                        Offset = varDesc.StartOffset,
                        Size = varDesc.Size,
                        Type = svt
                    });

                }

                ReflectionData.UniformBuffers.Add(bufferInfo);
            }

            for (int i = 0; i < desc.BoundResources; i++)
            {
                var res = reflection.GetResourceBindingDescription(i);

                switch (res.Type)
                {
                    case ShaderInputType.Texture:
                        {
                            ShaderResourceType type = res.Dimension switch
                            {
                                ShaderResourceViewDimension.Texture2D => ShaderResourceType.Texture2D,
                                ShaderResourceViewDimension.TextureCube => ShaderResourceType.TextureCube,
                                ShaderResourceViewDimension.Texture2DArray => ShaderResourceType.Texture2DArray,
                                ShaderResourceViewDimension.Texture3D => ShaderResourceType.Texture3D,

                                _ => throw new NotSupportedException(
                                    $"Unsupported texture dimension {res.Dimension}")
                            };

                            ReflectionData.Resources.Add(new ShaderResourceInfo
                            {
                                Name = res.Name,
                                BindSlot = res.BindPoint,
                                Stage = Stage,
                                Type = type
                            });

                            break;
                        }

                    case ShaderInputType.Sampler:
                        {
                            ReflectionData.Resources.Add(new ShaderResourceInfo
                            {
                                Name = res.Name,
                                BindSlot = res.BindPoint,
                                Stage = Stage,
                                Type = ShaderResourceType.Sampler
                            });
                            break;
                        }

                    case ShaderInputType.Structured:
                    case ShaderInputType.ByteAddress:
                        {
                            ReflectionData.Resources.Add(new ShaderResourceInfo
                            {
                                Name = res.Name,
                                BindSlot = res.BindPoint,
                                Stage = Stage,
                                Type = ShaderResourceType.StructuredBuffer
                            });
                            break;
                        }

                    case ShaderInputType.UnorderedAccessViewRWStructured:
                    case ShaderInputType.UnorderedAccessViewRWByteAddress:
                        {
                            ReflectionData.Resources.Add(new ShaderResourceInfo
                            {
                                Name = res.Name,
                                BindSlot = res.BindPoint,
                                Stage = Stage,
                                Type = ShaderResourceType.RwStructuredBuffer
                            });
                            break;
                        }

                    case ShaderInputType.UnorderedAccessViewRWTyped:
                        {
                            ShaderResourceType type = res.Dimension switch
                            {
                                ShaderResourceViewDimension.Texture2D => ShaderResourceType.RwTexture2D,
                                ShaderResourceViewDimension.Texture2DArray => ShaderResourceType.RwTexture2DArray,
                                ShaderResourceViewDimension.Texture3D => ShaderResourceType.RwTexture3D,

                                _ => throw new NotSupportedException(
                                    $"Unsupported UAV texture dimension {res.Dimension}")
                            };

                            ReflectionData.Resources.Add(new ShaderResourceInfo
                            {
                                Name = res.Name,
                                BindSlot = res.BindPoint,
                                Stage = Stage,
                                Type = type
                            });

                            break;
                        }

                    default:
                        {
                            if (res.Type != ShaderInputType.ConstantBuffer)
                                Console.WriteLine($"Unhandled shader resource '{res.Name}' : {res.Type}");
                            break;
                        }
                }
            }
            //PrintReflectionInfo(reflection);
        }

        private static int GetBindSlot(ShaderReflection reflection, string cbName)
        {
            var desc = reflection.Description;

            for (int i = 0; i < desc.BoundResources; i++)
            {
                var res = reflection.GetResourceBindingDescription(i);
                if (res.Name == cbName)
                    return res.BindPoint;
            }

            return -1;
        }

        private void PrintReflectionInfo(ShaderReflection reflection)
        {
            Console.WriteLine();
            Console.WriteLine("======================================================");
            Console.WriteLine($"Shader Reflection: {Name}");
            Console.WriteLine($"Stage : {Stage}");
            Console.WriteLine("======================================================");

            var shaderDesc = reflection.Description;

            Console.WriteLine($"Constant Buffers : {shaderDesc.ConstantBuffers}");
            Console.WriteLine($"Bound Resources  : {shaderDesc.BoundResources}");
            Console.WriteLine();

            // Constant Buffers
            for (int i = 0; i < shaderDesc.ConstantBuffers; i++)
            {
                var cb = reflection.GetConstantBuffer(i);
                var cbDesc = cb.Description;

                Console.WriteLine(
                    $"CBUFFER '{cbDesc.Name}'  Size={cbDesc.Size} bytes  Slot={GetBindSlot(reflection, cbDesc.Name)}");

                for (int v = 0; v < cbDesc.VariableCount; v++)
                {
                    var variable = cb.GetVariable(v);
                    var varDesc = variable.Description;
                    var type = variable.GetVariableType().Description;

                    Console.WriteLine(
                        $"    {varDesc.Name,-24} Offset={varDesc.StartOffset,3}  Size={varDesc.Size,3}  Type={type.Name}");
                }

                Console.WriteLine();
            }

            // Resources
            Console.WriteLine("Resources:");

            for (int i = 0; i < shaderDesc.BoundResources; i++)
            {
                var res = reflection.GetResourceBindingDescription(i);

                Console.WriteLine(
                    $"    {res.Name,-24} Type={res.Type,-18} Slot={res.BindPoint}");
            }

            Console.WriteLine("======================================================");
            Console.WriteLine();
        }

        private static string GetProfileForType(ShaderStage type)
        {
            return type switch
            {
                ShaderStage.Vertex => "vs_5_0",
                ShaderStage.Fragment => "ps_5_0",
                ShaderStage.Geometry => "gs_5_0",
                ShaderStage.Compute => "cs_5_0",
                _ => throw new NotSupportedException($"No profile for shader type: {type}")
            };
        }

        public void Dispose()
        {
            Vs?.Dispose();
            Ps?.Dispose();
            Cs?.Dispose();
            Gs?.Dispose();
            Bytecode = [];
        }
    }
}
