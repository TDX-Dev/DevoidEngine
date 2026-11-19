
using System.Text.Json.Serialization;

namespace DevoidEngine.Serialization
{
    [JsonSerializable(typeof(MaterialDescriptor))]
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
    public partial class MaterialJsonContext : JsonSerializerContext
    {
    }
}
