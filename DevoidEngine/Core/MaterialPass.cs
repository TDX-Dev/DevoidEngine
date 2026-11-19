using DevoidEngine.Logging;
using DevoidEngine.Serialization;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Core
{
    public sealed class MaterialPass : IDisposable
    {
        public Shader Shader { get; }
        public PipelineStateDescriptor PipelineState { get; }
        public Dictionary<string, UniformDefinitionDescriptor> UniformDefinitions { get; }

        public readonly int BufferBindSlot = -1;
        public readonly int BufferSize = 0;

        public readonly byte[] MaterialByteBuffer = null!;

        public Dictionary<string, ShaderVariableInfo> Variables { get; } = null!;
        public Dictionary<string, ShaderResourceInfo> TextureBindings { get; } = null!;
        public Dictionary<string, ShaderResourceInfo> SamplerBindings { get; } = null!;

        public Dictionary<string, Texture> Textures { get; } = null!;
        public Dictionary<string, Sampler> Samplers { get; } = null!;

        public MaterialPass(Shader shader, PipelineStateDescriptor pipelineState, Dictionary<string, UniformDefinitionDescriptor> uniforms)
        {
            Shader = shader;
            PipelineState = pipelineState;
            UniformDefinitions = uniforms;

            MaterialLayout? layout = ParseAndBuildMaterialLayout(shader.GetActivePass().ShaderReflectionData, uniforms);

            if (layout == null)
            {
                DevoidLog.Error(LogCategory.Core, "Unable to create material layout. This will cause issues if material buffer is being written to, as it has a size of 0.");
                return;
            }

            BufferSize = layout.BufferSize;
            BufferBindSlot = layout.BufferBindSlot;

            MaterialByteBuffer = new byte[BufferSize];

            Variables = layout.Variables;

            foreach (var texture in layout.Textures)
            {
                TextureBindings[texture.Key] = texture.Value;
                Textures[texture.Key] = Texture.Default;
            }

            foreach (var sampler in layout.Samplers)
            {
                SamplerBindings[sampler.Key] = sampler.Value;
                Samplers[sampler.Key] = Sampler.Default;
            }
        }

        MaterialLayout? ParseAndBuildMaterialLayout(ShaderReflectionData reflectionData, Dictionary<string, UniformDefinitionDescriptor> uniforms)
        {
            MaterialLayout layout = new();

            UniformDefinitionDescriptor? bufferDefinition = null;
            string bufferName = string.Empty;
            foreach (var uniform in uniforms)
            {
                if (uniform.Value.Type == UniformResourceType.ConstantBuffer)
                {
                    bufferDefinition = uniform.Value;
                    bufferName = uniform.Key;
                }
            }

            if (bufferDefinition != null && bufferName != string.Empty)
            {
                UniformBufferInfo? materialParamBuffer = reflectionData.UniformBuffers.FirstOrDefault(
                    x => string.Equals(
                        x.Name,
                        bufferName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (materialParamBuffer != null)
                {
                    layout.BufferName = materialParamBuffer.Name;
                    layout.BufferSize = materialParamBuffer.Size;
                    layout.BufferBindSlot = materialParamBuffer.BindSlot;

                    for (int i = 0; i < materialParamBuffer.Variables.Count; i++)
                    {
                        ShaderVariableInfo variableInfo = materialParamBuffer.Variables[i];
                        layout.Variables[variableInfo.Name] = variableInfo;
                    }
                }
            }

            foreach (var uniform in uniforms)
            {
                ShaderResourceInfo? binding = reflectionData.Resources.FirstOrDefault(x => x.Name == uniform.Key);

                if (binding == null)
                {
                    DevoidLog.Warning(LogCategory.Core, $"Error parsing uniform descriptor in material definition, {uniform.Key} was not found in shader.");
                    continue;
                }

                switch (binding.Type)
                {
                    case ShaderResourceType.Texture2D:
                    case ShaderResourceType.TextureCube:
                    case ShaderResourceType.Texture2DArray:
                    case ShaderResourceType.Texture3D:
                    case ShaderResourceType.RwTexture2D:
                    case ShaderResourceType.RwTexture2DArray:
                    case ShaderResourceType.RwTexture3D:

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

            if (layout.Variables.Count == 0 && (layout.Textures.Count == 0 && layout.Samplers.Count == 0))
            {
                DevoidLog.Warning(LogCategory.Core, $"Failed to build material layout: No definitions found.");
                return null;
            }

            return layout;
        }

        public byte[] GetMaterialBuffer() => MaterialByteBuffer;
        public Dictionary<string, ShaderResourceInfo> GetTextureBindings() => TextureBindings;
        public Dictionary<string, ShaderResourceInfo> GetSamplerBindings() => SamplerBindings;
        public bool HasTextureBinding(string name) => TextureBindings.ContainsKey(name);
        public bool HasSamplerBinding(string name) => SamplerBindings.ContainsKey(name);
        public Texture GetTexture(string name) => Textures[name];
        public Sampler GetSampler(string name) => Samplers[name];
        public bool TryGetVariable(string name, out ShaderVariableInfo? shaderVariableInfo) => Variables.TryGetValue(name, out shaderVariableInfo);
        public bool TryGetTextureBinding(string name, out ShaderResourceInfo? shaderResourceInfo) => TextureBindings.TryGetValue(name, out shaderResourceInfo);
        public bool TryGetSamplerBinding(string name, out ShaderResourceInfo? shaderResourceInfo) => SamplerBindings.TryGetValue(name, out shaderResourceInfo);

        #region SETTERS
        public void SetTexture(string name, Texture texture) => Textures[name] = texture ?? Texture.Default;
        public void SetSampler(string name, Sampler sampler) => Samplers[name] = sampler ?? Sampler.Default;
        public void SetInt(string name, int value) => Write(name, value);
        public void SetFloat(string name, float value) => Write(name, value);
        public void SetVector2(string name, Vector2 value) => Write(name, value);
        public void SetVector3(string name, Vector3 value) => Write(name, value);
        public void SetVector4(string name, Vector4 value) => Write(name, value);
        public void SetMatrix4X4(string name, Matrix4x4 value) => Write(name, value);
        #endregion

        private void Write<T>(string name, T value) where T : struct
        {
            if (!Variables.TryGetValue(name, out ShaderVariableInfo? varInfo))
            {
                DevoidLog.Warning(LogCategory.Core, $"Variable '{name}' not found in material.");
                return;
            }

            var span = MaterialByteBuffer.AsSpan(varInfo.Offset);
            MemoryMarshal.Write(span, value);
        }

        public void Dispose()
        {
            Shader.Dispose();
        }
    }
}
