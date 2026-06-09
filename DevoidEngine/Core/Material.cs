using DevoidEngine.Assets;
using DevoidGPU;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class Material : AssetType
    {
        public Shader Shader { get; }
        public BlendMode BlendMode { get; set; } = BlendMode.Opaque;
        public int MaterialBufferSize => materialBufferSize;
        public int MaterialBufferBindSlot => materialBufferBindSlot;


        private readonly Dictionary<string, ShaderVariableInfo> variables;
        private readonly Dictionary<string, TextureBindingInfo> textureBindings;

        private readonly Dictionary<string, Texture> textures;

        private readonly byte[] defaultBuffer = null!;

        private readonly int materialBufferBindSlot = -1;
        private readonly int materialBufferSize;

        public Material(Shader shader)
        {
            Shader = shader ?? throw new ArgumentNullException(nameof(shader));

            variables = [];
            textureBindings = [];
            textures = [];


            MaterialLayout? layout = shader.MaterialLayout;

            if (layout == null)
                return;

            materialBufferSize =
                    layout.BufferSize;

            materialBufferBindSlot =
                layout.BufferBindSlot;

            defaultBuffer =
                    new byte[layout.BufferSize];

            foreach (var variable in layout.Variables)
            {
                variables[variable.Key] =
                    variable.Value;
            }

            foreach (var texture in layout.Textures)
            {
                textureBindings[texture.Key] =
                    texture.Value;

                textures[texture.Key] = Texture.Default;
            }
        }

        public bool TryGetVariable(string name, out ShaderVariableInfo? info)
            => variables.TryGetValue(name, out info);

        public bool HasTextureBinding(string name)
            => textureBindings.ContainsKey(name);

        public TextureBindingInfo GetTextureBinding(string name)
            => textureBindings[name];

        public Texture GetDefaultTexture(string name)
            => textures[name];

        public ReadOnlySpan<byte> GetDefaultMaterialBuffer()
            => defaultBuffer;

        public Dictionary<string, TextureBindingInfo> GetTextureBindings()
            => textureBindings;

        public void SetTexture(string name, Texture texture)
        {

            if (!textureBindings.ContainsKey(name))
                throw new Exception($"Texture '{name}' not found in material layout.");

            textures[name] = texture ?? Texture.Default;
        }

        #region SETTERS

        public void SetInt(string name, int value)
            => Write(name, value);
        public void SetFloat(string name, float value)
        {
            Write(name, value);
        }

        public void SetVector2(string name, Vector2 value)
        {
            Write(name, value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            Write(name, value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            Write(name, value);
        }

        public void SetMatrix4x4(string name, Matrix4x4 value)
        {
            Write(name, value);
        }
        #endregion

        private void Write<T>(string name, T value) where T : struct
        {
            if (!variables.TryGetValue(name, out var varInfo))
            {
                Console.WriteLine($"Variable '{name}' not found in material.");
                return;
            }

            var span = defaultBuffer.AsSpan(varInfo.Offset);

            MemoryMarshal.Write(span, in value);
        }

    }
}
