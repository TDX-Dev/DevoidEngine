using DevoidEngine.Logging;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Core
{
    public sealed class MaterialInstance : IDisposable
    {
        public Material BaseMaterial { get; }
        public IDescriptorSet DescriptorSet { get => _descriptorSet; }

        private readonly Dictionary<string, Texture> _textureOverrides;
        private readonly Dictionary<string, Sampler> _samplerOverrides;

        private readonly HashSet<string> _overriddenProperties;
        private readonly HashSet<string> _overriddenTextures;
        private readonly HashSet<string> _overriddenSamplers;

        private readonly byte[]? _instanceBuffer; // CPU
        private readonly UniformBuffer? _instanceUniformBuffer; // GPU

        private readonly IDescriptorSet _descriptorSet;

        private bool _isDirty;

        public MaterialInstance(Material baseMaterial)
        {
            _textureOverrides = [];
            _samplerOverrides = [];
            _overriddenProperties = [];
            _overriddenTextures = [];
            _overriddenSamplers = [];


            BaseMaterial = baseMaterial;
            _descriptorSet = Engine.GraphicsDevice.CreateDescriptorSet(BaseMaterial.GetActiveShader().GetActivePass().DescriptorLayout);

            if (baseMaterial.Pass.BufferSize > 0)
            {
                _instanceBuffer = new byte[baseMaterial.Pass.BufferSize];
                BaseMaterial.Pass.GetMaterialBuffer().CopyTo(_instanceBuffer, 0);

                _instanceUniformBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)BaseMaterial.Pass.BufferSize);

                _descriptorSet.SetUniformBuffer((uint)BaseMaterial.Pass.BufferBindSlot, _instanceUniformBuffer.Gpu);
            }

            foreach (var kv in BaseMaterial.Pass.GetTextureBindings())
            {
                _descriptorSet.SetTexture((uint)kv.Value.BindSlot, BaseMaterial.Pass.GetTexture(kv.Key).Gpu);
            }

            foreach (var kv in BaseMaterial.Pass.GetSamplerBindings())
            {
                _descriptorSet.SetSampler((uint)kv.Value.BindSlot, BaseMaterial.Pass.GetSampler(kv.Key).Gpu);
            }

            _instanceUniformBuffer?.Update(_instanceBuffer);
        }

        private void UpdateBuffer()
        {
            if (!_isDirty || _instanceUniformBuffer == null || _instanceUniformBuffer == null)
                return;

            _instanceUniformBuffer.Update(_instanceBuffer);
            _isDirty = false;
        }

        public void SetInt(string name, int value) => Write(name, value);
        public void SetFloat(string name, float value) => Write(name, value);
        public void SetVector2(string name, Vector2 value) => Write(name, value);
        public void SetVector3(string name, Vector3 value) => Write(name, value);
        public void SetVector4(string name, Vector4 value) => Write(name, value);
        public void SetMatrix4X4(string name, Matrix4x4 value) => Write(name, value);

        private void Write<T>(string name, T value) where T : struct
        {
            if (!BaseMaterial.Pass.TryGetVariable(name, out ShaderVariableInfo? varInfo))
            {
                DevoidLog.Warning(LogCategory.Core, $"Variable '{name}' not found in material.");
                return;
            }

            var span = _instanceBuffer.AsSpan(varInfo!.Offset);
            MemoryMarshal.Write(span, value);

            _overriddenProperties.Add(name);
            _isDirty = true;
        }

        public void SetTexture(string name, Texture texture)
        {
            if (!BaseMaterial.Pass.TryGetTextureBinding(name, out ShaderResourceInfo? textureBinding))
            {
                DevoidLog.Warning(LogCategory.Core, $"Texture '{name}' not found in material layout.");
                return;
            }

            _textureOverrides.Add(name, texture);
            _overriddenTextures.Add(name);
            _descriptorSet.SetTexture((uint)textureBinding!.BindSlot, texture.Gpu);
        }

        public void SetSampler(string name, Sampler sampler)
        {
            if (!BaseMaterial.Pass.TryGetSamplerBinding(name, out ShaderResourceInfo? textureBinding))
            {
                DevoidLog.Warning(LogCategory.Core, $"Sampler '{name}' not found in material layout.");
                return;
            }

            _samplerOverrides.Add(name, sampler);
            _overriddenSamplers.Add(name);
            _descriptorSet.SetSampler((uint)textureBinding!.BindSlot, sampler.Gpu);
        }

        public void Dispose()
        {
            _instanceUniformBuffer?.Dispose();

            _textureOverrides.Clear();
            _samplerOverrides.Clear();
            _overriddenProperties.Clear();
            _overriddenTextures.Clear();
            _overriddenSamplers.Clear();
        }
    }
}
