using DevoidEngine.AssetPipeline;
using DevoidEngine.Components;
using DevoidEngine.Core;
using MessagePack;

namespace DevoidEngine.Assets
{
    [MessagePackObject]
    public class PackedScene
    {
        [Key(0)]
        public PackedSceneNode[] Nodes = [];
        [Key(1)]
        public Guid[] MeshGuids = [];
        [Key(2)]
        public Guid[] MaterialGuids = [];
        [Key(3)]
        public PackedCamera[] Cameras = [];
        [Key(4)]
        public PackedLight[] Lights = [];

        public Scene Instantiate()
        {
            Scene scene = new();

            List<GameObject> objects = new(Nodes.Length);

            // Pass 1
            foreach (var node in Nodes)
            {
                GameObject go = scene.AddGameObject(node.Name);

                go.Transform.LocalPosition = node.Translation;
                go.Transform.LocalRotation = node.Rotation;
                go.Transform.LocalScale = node.Scale;

                objects.Add(go);
            }

            // Pass 2
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].Parent != -1)
                    objects[i].SetParent(objects[Nodes[i].Parent]);
            }

            // Pass 3
            for (int i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];

                if (node.Meshes.Length == 0)
                    continue;

                foreach (var mr in node.Meshes)
                {
                    Mesh mesh =
                        Asset.Load<Mesh>(MeshGuids[mr.MeshIndex])!;

                    Material? material =
                        Asset.Load<Material>(MaterialGuids[mr.MaterialIndex]);

                    MeshRenderer renderer =
                        objects[i].AddComponent<MeshRenderer>();

                    renderer.Mesh = mesh;
                    renderer.Material = material == null ? null : new MaterialInstance(material);
                }
            }

            return scene;
        }
    }
}
