namespace DevoidEngine.Nodes
{
    [Flags]
    public enum NodeTickMode
    {
        None = 0,

        Play = 1 << 0,
        Edit = 1 << 1,

        All = Play | Edit
    }
}