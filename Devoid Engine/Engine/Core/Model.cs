using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Components;
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
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.AddMesh(Meshes[meshIndex]);
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
