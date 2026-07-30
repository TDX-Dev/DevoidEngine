using MessagePack;

namespace DevoidEngine.Serialization
{
    [MessagePackObject]
    public class ComponentData
    {
        [Key(0)] public string Type = "Component";

        [Key(1)] public byte[] Data = [];
    }
}
