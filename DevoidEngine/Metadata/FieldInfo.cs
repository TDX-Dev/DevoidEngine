namespace DevoidEngine.Metadata
{
    public sealed class FieldInfo
    {
        public required string Name { get; init; }
        public required Type FieldType { get; init; }

        public required FieldGetter Getter { get; init; }
        public FieldSetter? Setter { get; init; }
    }
}