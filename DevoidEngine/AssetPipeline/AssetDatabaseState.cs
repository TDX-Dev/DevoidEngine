using MessagePack;

namespace DevoidEngine.AssetPipeline
{
    [MessagePackObject]
    public class AssetDatabaseState
    {
        [Key(0)]
        public Dictionary<Guid, AssetEntry> Entries = [];
    }
}
