using System.Text.Json.Serialization;

namespace DevoidEngine.Core
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ProjectData))]
    internal partial class ProjectJsonContext : JsonSerializerContext
    {
    }
}
