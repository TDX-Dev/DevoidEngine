namespace DevoidEngine.AssetPipeline
{
    public interface IAssetImporter
    {
        string Name { get; }
        IReadOnlyList<string> Extensions { get; }
        string OutputExtension { get; }
        Type SettingsType { get; }
        int SettingsVersion { get; }
        int Priority { get; }
        bool IsAssetFolderOnly { get; }


        byte[] CreateDefaultSettings();
        void Import(ImportContext importContext, byte[] settingsData);
    }
}
