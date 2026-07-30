namespace DevoidEngine.AssetPipeline.Importers
{
    internal class SceneImporter : AssetImporter<SceneImportSettings>
    {
        public override string Name => "SceneImporter";

        public override IReadOnlyList<string> Extensions => [".scene"];

        public override string OutputExtension => "scene";

        public override SceneImportSettings DefaultSettings()
        {
            return new SceneImportSettings();
        }

        public override void Import(ImportContext context, SceneImportSettings settings)
        {
            // Do Nothing
        }
    }
}
