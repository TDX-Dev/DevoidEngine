using System.Text.Json.Serialization;

namespace DevoidEngine.Core
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ProjectData))]
    [JsonSerializable(typeof(ProjectSettings))]
    [JsonSerializable(typeof(GameSettings))]
    [JsonSerializable(typeof(InputSettings))]
    [JsonSerializable(typeof(PhysicsSettings))]
    internal partial class ProjectJsonContext : JsonSerializerContext
    {
    }
}
