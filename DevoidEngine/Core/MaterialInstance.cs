using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class MaterialInstance : IDisposable
    {
        public Material BaseMaterial { get; }
        public IDescriptorSet DescriptorSet
        {
            get
            {
                UpdateBuffer();
                return descriptorSet;
            }
        }

        private readonly Dictionary<string, Texture> textureOverrides;

        private readonly byte[] cpuBuffer;
        private readonly UniformBuffer gpuBuffer;

        private readonly IDescriptorSet descriptorSet;

        private bool isDirty;
        private bool disposed;
        private readonly ShaderPass defaultPass;

        public MaterialInstance(Material material)
        {
            BaseMaterial = material;

            cpuBuffer = new byte[BaseMaterial.MaterialBufferSize];
            BaseMaterial.GetDefaultMaterialBuffer().CopyTo(cpuBuffer);

            gpuBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Math.Max(1, BaseMaterial.MaterialBufferSize));

            descriptorSet =
                Engine.GraphicsDevice.CreateDescriptorSet(BaseMaterial.Shader.GetPass("Forward").DescriptorLayout);

            textureOverrides = [];
            isDirty = true;

            descriptorSet.SetUniformBuffer((uint)BaseMaterial.MaterialBufferBindSlot, gpuBuffer.GPU);

            foreach (var kv in BaseMaterial.GetTextureBindings())
            {
                string name = kv.Key;
                TextureBindingInfo binding = kv.Value;

                Texture texture =
                    BaseMaterial.GetDefaultTexture(name);

                descriptorSet.SetTexture(
                    (uint)binding.BindSlot,
                    texture.GPU);
            }

            defaultPass = BaseMaterial.Shader.GetPass(BaseMaterial.Shader.ShaderDescriptor.DefaultPass);

        }

        private void UpdateBuffer()
        {
            if (!isDirty) return;

            gpuBuffer.Update(cpuBuffer);

            isDirty = false;
        }

        public void SetInt(string name, int value)
            => Write(name, value);

        public void SetFloat(string name, float value)
            => Write(name, value);

        public void SetVector2(string name, Vector2 value)
            => Write(name, value);

        public void SetVector3(string name, Vector3 value)
            => Write(name, value);

        public void SetVector4(string name, Vector4 value)
            => Write(name, value);

        public void SetMatrix4x4(string name, Matrix4x4 value)
            => Write(name, value);

        private void Write<T>(string name, T value) where T : struct
        {
            if (!BaseMaterial.TryGetVariable(name, out var varInfo))
            {
                Console.WriteLine($"Variable '{name}' not found in material.");
                return;
            }

            var span = cpuBuffer.AsSpan(varInfo!.Offset);
            MemoryMarshal.Write(span, in value);

            isDirty = true;
        }

        public void SetTexture(string name, Texture texture)
        {
            if (!BaseMaterial.HasTextureBinding(name))
            {
                Console.WriteLine($"Texture '{name}' not found in material layout.");
                return;
            }

            textureOverrides[name] = texture ?? Texture.Default;

            TextureBindingInfo binding = BaseMaterial.GetTextureBinding(name);

            descriptorSet.SetTexture((uint)binding.BindSlot, texture?.GPU ?? Texture.Default.GPU);
        }

        public void ClearTextureOverride(string name)
        {
            textureOverrides.Remove(name);

            TextureBindingInfo binding =
                BaseMaterial.GetTextureBinding(name);

            descriptorSet.SetTexture(
                (uint)binding.BindSlot,
                BaseMaterial.GetDefaultTexture(name).GPU);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            gpuBuffer?.Dispose();
            textureOverrides.Clear();

            disposed = true;

            GC.SuppressFinalize(this);
        }

        ~MaterialInstance()
        {
            Dispose();
        }
    }
}
