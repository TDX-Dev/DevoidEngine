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
    public class Model : AssetType
    {
        public Mesh[] Meshes = [];
        public ModelNode[] Nodes = [];
        public Material[] Materials = [];
        public int[] MeshMaterialIndices = [];

        //public GameObject Instantiate(GameObject root)
        //{
        //    GameObject[] objects = new GameObject[Nodes.Length];

        //    for (int i = 0; i < Nodes.Length; i++)
        //    {
        //        var node = Nodes[i];

        //        GameObject go = root.Scene.AddGameObject("Child");
        //        go.SetParent(root);
        //        //go.Transform.SetParent(root.Transform);

        //        objects[i] = go;

        //        var transform = go.Transform;
        //        transform.LocalPosition = node.Translation;
        //        transform.LocalRotation = node.Rotation;
        //        transform.LocalScale = node.Scale;

        //        foreach (int meshIndex in node.MeshIndices)
        //        {
        //            var mesh = Meshes[meshIndex];

        //            int materialIndex = MeshMaterialIndices[meshIndex];
        //            var material = Materials[materialIndex];

        //            Renderer.SkyboxRenderer.BindIBL(material);

        //            var renderer = go.AddComponent<MeshRenderer>();
        //            renderer.AddMesh(mesh);
        //            renderer.AddMaterial(new MaterialInstance(material));
        //        }
        //    }

        //    // build hierarchy
        //    for (int i = 0; i < Nodes.Length; i++)
        //    {
        //        int parent = Nodes[i].Parent;

        //        if (parent >= 0)
        //            objects[i].Transform.SetParent(objects[parent].Transform);
        //    }

        //    return objects[0];
        //}

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
