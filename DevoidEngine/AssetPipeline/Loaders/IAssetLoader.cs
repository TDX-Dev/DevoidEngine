namespace DevoidEngine.AssetPipeline.Loaders
{
    public interface IAssetLoader<T>
    {
        T Load(ReadOnlySpan<byte> data);

    }
}
