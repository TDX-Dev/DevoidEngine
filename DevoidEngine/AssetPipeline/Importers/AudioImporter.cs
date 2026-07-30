namespace DevoidEngine.AssetPipeline.Importers
{
    internal class AudioImporter : AssetImporter<AudioImportSettings>
    {
        public override string Name => "AudioImporter";

        public override IReadOnlyList<string> Extensions =>
            [".mp3", ".wav", ".ogg"];

        public override string OutputExtension => "audio";

        public override AudioImportSettings DefaultSettings()
        {
            return new AudioImportSettings();
        }

        public override void Import(
            ImportContext context,
            AudioImportSettings settings
        )
        {
            Console.WriteLine($"Importing audio {context.AssetPath}");

            var bytes = File.ReadAllBytes(context.AssetPath);

            File.WriteAllBytes(context.GetRootOutputPath(context.OutputExtension), bytes);
        }

        public override bool Exists(ImportContext context)
        {
            return File.Exists(context.GetRootOutputPath(OutputExtension));
        }
    }
}
