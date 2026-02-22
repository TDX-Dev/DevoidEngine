using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class MaterialInstance
    {
        public Material BaseMaterial { get; }

        private Dictionary<string, Texture> textureOverrides;

        private byte[] cpuBuffer;
        private UniformBuffer gpuBuffer;
        private bool isDirty;

        public MaterialInstance(Material material)
        {
            BaseMaterial = material;

            cpuBuffer = new byte[BaseMaterial.MaterialBufferSize];
            BaseMaterial.GetDefaultMaterialBuffer().CopyTo(cpuBuffer);

            gpuBuffer = new UniformBuffer(BaseMaterial.MaterialBufferSize == 0 ? 1 : BaseMaterial.MaterialBufferSize, BufferUsage.Dynamic);

            textureOverrides = new Dictionary<string, Texture>();
            isDirty = true;

        }

        private void UpdateBuffer()
        {
            if (!isDirty) return;

            gpuBuffer.SetData(cpuBuffer);

            isDirty = false;
        }

        public void Bind()
        {
            UpdateBuffer();

            gpuBuffer.Bind(BaseMaterial.MaterialBufferBindSlot);
            BaseMaterial.Shader.Use();

            foreach (var texName in BaseMaterial.GetTextureNames())
            {
                var binding = BaseMaterial.GetTextureBinding(texName);

                Texture texture =
                    textureOverrides.TryGetValue(texName, out var overrideTex)
                        ? overrideTex
                        : BaseMaterial.GetDefaultTexture(texName);

                texture.Bind(binding.BindSlot);
            }
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

            var span = cpuBuffer.AsSpan(varInfo.Offset);
            MemoryMarshal.Write(span, ref value);

            isDirty = true;
        }

        public void SetTexture(string name, Texture texture)
        {
            if (!BaseMaterial.HasTextureBinding(name))
                throw new Exception($"Texture '{name}' not found in material layout.");

            textureOverrides[name] = texture ?? Texture2D.WhiteTexture;
        }

        public void ClearTextureOverride(string name)
        {
            textureOverrides.Remove(name);
        }
    }
}
