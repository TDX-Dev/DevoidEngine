namespace DevoidEngine.AssetPipeline.Loaders
{
    public interface IAssetLoader<T>
    {
        string RuntimeExtension { get; }
        T Load(byte[] data);

    }
}
