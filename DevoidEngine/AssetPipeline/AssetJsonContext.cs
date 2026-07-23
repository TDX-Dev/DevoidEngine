using System.Text.Json.Serialization;

namespace DevoidEngine.AssetPipeline
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        IncludeFields = true
    )]
    [JsonSerializable(typeof(AssetMeta))]
    internal partial class AssetJsonContext : JsonSerializerContext
    {
    }
}
