using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public class Model : AssetType, ISubAssetContainer
    {
        public Mesh[] Meshes = [];
        public ModelNode[] Nodes = [];
        public Material[] Materials = [];
        public int[] MeshMaterialIndices = [];

        public bool TryGetSubAsset<T>(ulong id, out T asset) where T : class?
        {
            if (typeof(T) == typeof(Mesh))
            {
                if (id < (ulong)Meshes.Length)
                {
                    asset = (Meshes[(int)id] as T)!;
                    return true;
                }
            }

            if (typeof(T) == typeof(Material))
            {
                ulong matId = id - (ulong)Meshes.Length;

                if (matId < (ulong)Materials.Length)
                {
                    asset = (Materials[(int)matId] as T)!;
                    return true;
                }
            }

            asset = null!;
            return false;
        }

        public GameObject Instantiate(Scene scene)
        {
            GameObject[] objects = new GameObject[Nodes.Length];

            for (int i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];

                GameObject go = scene.AddGameObject(node.Name);
                objects[i] = go;

                var transform = go.Transform;

                transform.LocalPosition = node.Translation;
                transform.LocalRotation = node.Rotation;
                transform.LocalScale = node.Scale;

                foreach (int meshIndex in node.MeshIndices)
                {
                    var mesh = Meshes[meshIndex];
                    var material = Materials[MeshMaterialIndices[meshIndex]];
                    Renderer.SkyboxRenderer.BindIBL(material);

                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.AddMesh(mesh);
                    renderer.AddMaterial(new MaterialInstance(material));
                }
            }

            for (int i = 0; i < Nodes.Length; i++)
            {
                int parent = Nodes[i].Parent;

                if (parent >= 0)
                    objects[i].SetParent(objects[parent]);
            }

            return objects[0];
        }
    }
}
