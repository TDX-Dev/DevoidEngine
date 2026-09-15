namespace DevoidEngine.Metadata
{
    public sealed class PropertyInfo
    {
        public required string Name { get; init; }
        public required Type PropertyType { get; init; }

        public required PropertyGetter Getter { get; init; }
        public PropertySetter? Setter { get; init; }
    }
}
