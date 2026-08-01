using DevoidEngine.Core;
using System.Text.Json.Serialization;

namespace DevoidEngine.Serialization
{
    [JsonSerializable(typeof(ShaderDescriptor))]
    [JsonSourceGenerationOptions(
    UseStringEnumConverter = true)]
    public partial class ShaderJsonContext : JsonSerializerContext
    {
    }
}
