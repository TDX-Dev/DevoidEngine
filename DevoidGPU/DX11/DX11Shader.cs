using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Text.RegularExpressions;

namespace DevoidGPU.DX11
{
    public class DX11Shader : IShader
    {
        public ShaderType Type { get; private set; }
        public ShaderReflectionData ReflectionData { get; private set; }

        public string Name { get; private set; }

        public VertexShader VertexShader { get; private set; }
        public PixelShader PixelShader { get; private set; }
        public GeometryShader GeometryShader { get; private set; }
        public ComputeShader ComputeShader { get; private set; }

        public byte[] ByteCode { get; private set; }
        private readonly Device device;

        public DX11Shader(Device device, ShaderType type, string name = null)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            this.Type = type;
            this.Name = name ?? "UnnamedShader";
        }


        public void Compile(string source, string entryPoint, string path)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Shader source cannot be empty.", nameof(source));

            if (string.IsNullOrWhiteSpace(entryPoint))
                throw new ArgumentException("Entry point cannot be empty.", nameof(entryPoint));

            string profile = GetProfileForType(Type);

            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#endif
            CompilationResult result;
            if (string.IsNullOrEmpty(path))
            {
                source = RemoveIncludes(source);
                result = ShaderBytecode.Compile(source, entryPoint, profile, flags);
            }
            else
            {
                result = ShaderBytecode.Compile(source, entryPoint, profile, flags, EffectFlags.None, new ShaderMacro[] { }, new DX11ShaderIncludeHandler(path));
            }
            if (result.HasErrors)
                throw new Exception($"Shader compile error ({Name}): {result.Message}");

            ByteCode = result.Bytecode;

            CreateReflectionInfoFromBytecode();
            CreateShaderFromBytecode();
        }

        public string RemoveIncludes(string source)
        {
            return Regex.Replace(
                source,
                @"^\s*#include\s+[<""][^>""]+[>""]\s*$",
                "",
                RegexOptions.Multiline
            );
        }

        public void LoadPrecompiled(byte[] bytecode)
        {
            using (var shaderBytecode = new SharpDX.D3DCompiler.ShaderBytecode(bytecode))
            {
                ByteCode = shaderBytecode;
                CreateShaderFromBytecode();
            }
        }

        private void CreateShaderFromBytecode()
        {
            switch (Type)
            {
                case ShaderType.Vertex:
                    VertexShader = new VertexShader(device, ByteCode);
                    break;
                case ShaderType.Fragment:
                    PixelShader = new PixelShader(device, ByteCode);
                    break;
                case ShaderType.Geometry:
                    GeometryShader = new GeometryShader(device, ByteCode);
                    break;
                case ShaderType.Compute:
                    ComputeShader = new ComputeShader(device, ByteCode);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported shader type: {Type}");
            }
        }

        private static string GetProfileForType(ShaderType type)
        {
            return type switch
            {
                ShaderType.Vertex => "vs_5_0",
                ShaderType.Fragment => "ps_5_0",
                ShaderType.Geometry => "gs_5_0",
                ShaderType.Compute => "cs_5_0",
                _ => throw new NotSupportedException($"No profile for shader type: {type}")
            };
        }

        private void CreateReflectionInfoFromBytecode()
        {
            ReflectionData = new ShaderReflectionData();

            ShaderReflection reflection = new ShaderReflection(ByteCode);
            ShaderDescription desc = reflection.Description;

            // Fill in constant buffers
            for (int i = 0; i < desc.ConstantBuffers; i++)
            {
                ConstantBuffer cb = reflection.GetConstantBuffer(i);
                ConstantBufferDescription cbDesc = cb.Description;

                UniformBufferInfo BufferInfo = new UniformBufferInfo()
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
                    var svt = DX11ReflectionMapper.ConvertResourceType(typeDesc.Name);

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
                        Stage = DX11StateMapper.ToDXShaderStage(Type),
                        ArraySize = 1
                    });

                }
            }
            //PrintReflectionInfo(reflection);
        }

        private void PrintReflectionInfo(ShaderReflection reflection)
        {
            Console.WriteLine("==== Shader Reflection ====");
            Console.WriteLine($"Stage: {Type}");
            Console.WriteLine();

            // Constant Buffers
            foreach (var cb in ReflectionData.UniformBuffers)
            {
                Console.WriteLine($"CBUFFER: {cb.Name}");
                Console.WriteLine($"  Size     : {cb.Size}");
                Console.WriteLine($"  BindSlot : b{cb.BindSlot}");
                Console.WriteLine("  Variables:");

                foreach (var v in cb.Variables)
                {
                    Console.WriteLine($"    {v.Name}");
                    Console.WriteLine($"      Offset : {v.Offset}");
                    Console.WriteLine($"      Size   : {v.Size}");
                    Console.WriteLine($"      Type   : {v.Type}");
                }

                Console.WriteLine();
            }

            // Textures
            foreach (var tex in ReflectionData.TextureBindings)
            {
                Console.WriteLine($"TEXTURE: {tex.Name}");
                Console.WriteLine($"  BindSlot : t{tex.BindSlot}");
                Console.WriteLine($"  Stage    : {tex.Stage}");
                Console.WriteLine();
            }

            Console.WriteLine("============================");
        }

        private int GetBindSlot(ShaderReflection reflection, string cbName)
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




        public void Dispose()
        {
            VertexShader?.Dispose();
            PixelShader?.Dispose();
            GeometryShader?.Dispose();
            ComputeShader?.Dispose();
        }


    }
}
