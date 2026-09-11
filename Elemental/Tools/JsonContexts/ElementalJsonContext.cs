using DevoidEngine.Core;
using Elemental.Tools.EditorConfiguration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elemental.Tools.JsonContexts
{

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true,PropertyNameCaseInsensitive = true, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(EditorConfig))]
    public partial class DevoidJsonContext : JsonSerializerContext
    {
    }
}
