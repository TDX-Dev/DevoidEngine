using DevoidEngine.Assets;
using DevoidEngine.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.AssetPipeline.Loaders
{
    internal class MaterialLoader : IAssetLoader<Material>
    {
        public string RuntimeExtension => "material";

        public Material Load(ReadOnlySpan<byte> data)
        {
            MaterialAsset asset;

            try
            {
                asset = MessagePackSerializer.Deserialize<MaterialAsset>(data.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Material Loader]: Error Loading Material {e.Message}");
                throw new Exception();
            }

            Shader shader = Engine.Renderer.DefaultShader;
            Material material = new(shader);

            foreach (var (name, guid) in asset.Textures)
            {
                Texture? tex = Asset.Load<Texture>(guid);
                if (tex != null)
                    material.SetTexture(name, tex);
            }
            foreach (var (name, value) in asset.Ints)
                material.SetInt(name, value);

            foreach (var (name, value) in asset.Floats)
            {
                material.SetFloat(name, value);
                if (name == "NormalStrength")
                {
                    Console.WriteLine("Loaded Normal Strength: " + value);
                }
            }

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
