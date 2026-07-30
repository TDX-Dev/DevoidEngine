using MessagePack;

namespace DevoidEngine.Serialization
{
    [MessagePackObject]
    public class SceneData
    {
        [Key(0)]
        public List<GameObjectData> GameObjects = [];
    }
}
