using DevoidEngine.AssetPipeline;
using DevoidEngine.Components;
using DevoidEngine.Core;
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

            List<GameObject> objects = new(Nodes.Length);
            foreach (var node in Nodes)
            {
                GameObject go = scene.AddGameObject(node.Name);

                go.Transform.LocalPosition = node.Translation;
                go.Transform.LocalRotation = node.Rotation;
                go.Transform.LocalScale = node.Scale;

                objects.Add(go);
            }

            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].Parent != -1)
                    objects[i].SetParent(objects[Nodes[i].Parent]);
            }
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

                    StaticColliderComponent staticColliderComponent = objects[i].AddComponent<StaticColliderComponent>();
                    staticColliderComponent.Shape = new Physics.PhysicsShapeDescription()
                    {
                        Type = Physics.PhysicsShapeType.Mesh,
                        Vertices = mesh.Positions!,
                        Indices = Array.ConvertAll(mesh.Indices!, x => (int)x)
                    };

                    renderer.Mesh = mesh;
                    renderer.Material = material == null ? null : new MaterialInstance(material);
                }
            }

            for (int i = 0; i < Nodes.Length; i++)
            {
                PackedLight? packed = Nodes[i].Light;

                if (packed == null)
                    continue;

                Console.WriteLine("Added Lights");

                LightComponent light = objects[i].AddComponent<LightComponent>();

                switch (packed.Type)
                {
                    case Rendering.LightType.DirectionalLight:
                        light.LightType = Rendering.LightType.DirectionalLight;
                        break;

                    case Rendering.LightType.PointLight:
                        light.LightType = Rendering.LightType.PointLight;
                        break;

                    case Rendering.LightType.SpotLight:
                        light.LightType = Rendering.LightType.SpotLight;
                        break;
                }

                light.Color = new Vector4(packed.Color, 1.0f);
                light.Intensity = packed.Intensity;
                light.Radius = packed.Range;

                if (packed.Type == Rendering.LightType.SpotLight)
                {
                    light.InnerCutoff = MathHelper.RadToDeg(packed.InnerCone);
                    light.OuterCutoff = MathHelper.RadToDeg(packed.OuterCone);
                }
            }

            return scene;
        }
    }
}
