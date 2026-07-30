namespace DevoidEngine.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DontSerialize : Attribute
    {
    }
}
