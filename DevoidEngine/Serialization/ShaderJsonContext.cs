using DevoidEngine.Core;
using System.Text.Json.Serialization;

namespace DevoidEngine.Serialization
{
    [JsonSerializable(typeof(ShaderDescriptor))]
    public partial class ShaderJsonContext : JsonSerializerContext
    {
    }
}
