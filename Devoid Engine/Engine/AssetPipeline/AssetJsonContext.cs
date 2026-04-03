using System.Text.Json.Serialization;

namespace DevoidEngine.Engine.AssetPipeline
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