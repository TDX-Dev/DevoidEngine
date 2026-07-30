namespace DevoidEngine.Core
{
    public interface IVirtualFileSource
    {
        bool Exists(string path);

        Stream OpenRead(string path);

        IEnumerable<string> Enumerate(string path);
    }
}
