using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ProjectSystem
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ProjectConfig))]
    internal partial class ProjectJsonContext : JsonSerializerContext
    {
    }
}
