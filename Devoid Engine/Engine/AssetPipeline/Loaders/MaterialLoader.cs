using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Loaders
{
    public class MaterialLoader : IAssetLoader<Material>
    {
        public Material Load(ReadOnlySpan<byte> data)
        {
            var asset = MessagePackSerializer.Deserialize<MaterialAsset>(data.ToArray());

            Shader? shader = ShaderLibrary.GetShader(asset.Shader);

            if (shader == null)
            {
                Console.WriteLine($"[Material Loader]: Failed to load shader {asset.Shader}, reverting to PBR");
                shader = ShaderLibrary.GetShader("PBR/ForwardPBR")!;
            }

            Material material = new Material(shader);

            foreach (var (name, guid) in asset.Textures)
            {
                Texture2D? tex = AssetManager.Load<Texture2D>(guid);
                if (tex == null)
                {
                    Console.WriteLine($"[Material Loader]: Failed to load texture: {name} -> {guid}");
                    continue;
                }
                material.SetTexture(name, tex);
            }

            foreach (var (name, value) in asset.Ints)
                material.SetInt(name, value);

            foreach (var (name, value) in asset.Floats)
                material.SetFloat(name, value);

            foreach (var (name, value) in asset.Vector2s)
                material.SetVector2(name, value);

            foreach (var (name, value) in asset.Vector3s)
                material.SetVector3(name, value);

            foreach (var (name, value) in asset.Vector4s)
                material.SetVector4(name, value);

            foreach (var (name, value) in asset.Matrices)
                material.SetMatrix4x4(name, value);

            return material;
        }
    }
}
