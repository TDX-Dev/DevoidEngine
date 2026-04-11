using DevoidEngine.Engine.InputSystem;
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
    [JsonSerializable(typeof(ProjectSettings))]
    [JsonSerializable(typeof(InputAction))]
    [JsonSerializable(typeof(List<InputAction>))]
    [JsonSerializable(typeof(InputBinding))]
    [JsonSerializable(typeof(List<InputBinding>))]
    internal partial class ProjectJsonContext : JsonSerializerContext
    {
    }
}
