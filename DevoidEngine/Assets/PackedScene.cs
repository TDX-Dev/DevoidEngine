using DevoidEngine.AssetPipeline;
using DevoidEngine.Core;
using DevoidEngine.Nodes;
using DevoidEngine.Util;
using MessagePack;
using System.Numerics;

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

        public Scene Instantiate(Scene? existing = null)
        {
            Scene scene = existing ?? new();

            List<Node3D> nodes = new(Nodes.Length);

            // Instantiate the actual node types.
            foreach (var packed in Nodes)
            {
                Node3D node;

                if (packed.Mesh != null)
                {
                    MeshNode meshNode = scene.CreateNode<MeshNode>();
                    node = meshNode;

                    Mesh mesh = Asset.Load<Mesh>(
                        MeshGuids[packed.Mesh.MeshIndex])!;

                    for (int j = 0; j < packed.Mesh.MaterialGuids.Length; j++)
                    {
                        Material material = Asset.Load<Material>(packed.Mesh.MaterialGuids[j])!;
                        mesh.Surfaces[j].Material = new MaterialInstance(material);
                    }

                    meshNode.Mesh = mesh;
                }
                else if (packed.Light != null)
                {
                    LightNode lightNode = scene.CreateNode<LightNode>();
                    node = lightNode;

                    lightNode.LightType = packed.Light.Type;
                    lightNode.Color = new Vector4(packed.Light.Color, 1.0f);
                    lightNode.Intensity = packed.Light.Intensity * 0.001f;
                    lightNode.Radius = 100;// packed.Light.Range;

                    if (packed.Light.Type == Rendering.LightType.SpotLight)
                    {


                        //lightNode.InnerCutoff = MathHelper.RadToDeg(packed.Light.InnerCone);

                        //lightNode.OuterCutoff =  MathHelper.RadToDeg(packed.Light.OuterCone);

                        Console.WriteLine(lightNode.innerCutoff);
                        Console.WriteLine(lightNode.outerCutoff);
                    }
                }
                else
                {
                    node = scene.CreateNode<Node3D>();
                }

                node.Name = packed.Name;
                node.Transform.LocalPosition = packed.Translation;
                node.Transform.LocalRotation = packed.Rotation;
                node.Transform.LocalScale = packed.Scale;

                nodes.Add(node);
            }

            // Reconstruct hierarchy.
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].Parent != -1)
                    nodes[i].SetParent(nodes[Nodes[i].Parent]);
            }

            return scene;
        }
    }
}
