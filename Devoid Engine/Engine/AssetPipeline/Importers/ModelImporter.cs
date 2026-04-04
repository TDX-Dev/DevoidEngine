using DevoidEngine.Engine.Assets;
using DevoidEngine.Engine.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.AssetPipeline.Importers
{
    public class ModelImporter : AssetImporter<ModelImportSettings>
    {
        public override string Name => "ModelImporter";

        public override IReadOnlyList<string> Extensions =>
            new[] { ".fbx", ".gltf", ".glb", ".obj" };

        public override string OutputExtension => "model";

        public override ModelImportSettings DefaultSettings()
        {
            return new ModelImportSettings();
        }

        public override void Import(
            string assetPath,
            Guid guid,
            ModelImportSettings settings,
            string outputPath)
        {
            Console.WriteLine($"Importing model {assetPath}");

            AssimpContext ctx = new();

            var scene = ctx.ImportFile(assetPath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.CalculateTangentSpace);

            var model = ConvertScene(scene);

            File.WriteAllBytes(
                outputPath,
                MessagePackSerializer.Serialize(model));
        }

        private ModelAsset ConvertScene(Scene scene)
        {
            List<ModelNode> nodes = new();
            List<MeshAsset> meshes = new();

            ProcessNode(scene.RootNode, -1, nodes, meshes, scene);

            return new ModelAsset
            {
                Nodes = nodes.ToArray(),
                Meshes = meshes.ToArray(),
                Materials = []
            };
        }
    }
}
