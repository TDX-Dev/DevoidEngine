using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal partial class DX11Shader : IShader
    {
        public ShaderDescription Description { get; }
        public ShaderStage Stage { get; }
        public ShaderReflectionData ReflectionData { get; private set; } = null!;
        public string Name { get; }

        internal byte[] bytecode = null!;

        internal VertexShader? VS;
        internal PixelShader? PS;
        internal GeometryShader? GS;
        internal ComputeShader? CS;

        private readonly Device device;

        public DX11Shader(Device dx11Device, ShaderDescription desc)
        {
            device = dx11Device;
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
            result = ShaderBytecode.Compile(source, entryPoint, profile, flags, EffectFlags.None, shaderDefines, new DX11ShaderIncludeHandler(path));
            if (result.HasErrors)
                throw new Exception($"Shader compile error ({Name}): {result.Message}");

            bytecode = result.Bytecode;

            CreateReflectionInfoFromBytecode();
            CreateShaderFromBytecode();
        }

        private void CreateShaderFromBytecode()
        {
            switch (Stage)
            {
                case ShaderStage.Vertex:
                    VS = new VertexShader(device, bytecode);
                    break;
                case ShaderStage.Fragment:
                    PS = new PixelShader(device, bytecode);
                    break;
                case ShaderStage.Geometry:
                    GS = new GeometryShader(device, bytecode);
                    break;
                case ShaderStage.Compute:
                    CS = new ComputeShader(device, bytecode);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported shader type: {Stage}");
            }
        }


        private void CreateReflectionInfoFromBytecode()
        {
            ReflectionData = new ShaderReflectionData();

            ShaderReflection reflection = new(bytecode);
            SharpDX.D3DCompiler.ShaderDescription desc = reflection.Description;

            // Fill in constant buffers
            for (int i = 0; i < desc.ConstantBuffers; i++)
            {
                ConstantBuffer cb = reflection.GetConstantBuffer(i);
                ConstantBufferDescription cbDesc = cb.Description;

                UniformBufferInfo BufferInfo = new()
                {
                    Name = cbDesc.Name,
                    Size = cbDesc.Size,
                    BindSlot = GetBindSlot(reflection, cbDesc.Name)
                };

                for (int v = 0; v < cbDesc.VariableCount; v++)
                {
                    var variable = cb.GetVariable(v);
                    var varDesc = variable.Description;
                    var varType = variable.GetVariableType();
                    var typeDesc = varType.Description;
                    var svt = DX11StateMapper.ConvertResourceType(typeDesc.Name);

                    BufferInfo.Variables.Add(new ShaderVariableInfo
                    {
                        Name = varDesc.Name,
                        Offset = varDesc.StartOffset,
                        Size = varDesc.Size,
                        Type = svt
                    });

                }

                ReflectionData.UniformBuffers.Add(BufferInfo);
            }

            for (int i = 0; i < desc.BoundResources; i++)
            {
                var resc = reflection.GetResourceBindingDescription(i);

                if (resc.Type == ShaderInputType.Texture)
                {
                    ReflectionData.TextureBindings.Add(new TextureBindingInfo()
                    {
                        Name = resc.Name,
                        BindSlot = resc.BindPoint,
                        Stage = Stage,
                        ArraySize = 1
                    });

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
    }
}
