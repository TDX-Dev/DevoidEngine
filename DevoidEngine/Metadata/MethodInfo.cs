namespace DevoidEngine.Metadata
{
    public sealed class MethodInfo
    {
        public required string Name { get; init; } = string.Empty;
        public Type[] ArgTypes { get; init; } = [];
        public required Type ReturnType { get; init; } = null!;
        public required MethodInvoker Invoker { get; init; }

    }
}
