using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DevoidEngine.SourceGen.ComponentSerialization;

internal static class RegistryEmitter
{
    public static void Emit(
        SourceProductionContext context,
        ImmutableArray<INamedTypeSymbol> components)
    {
        StringBuilder sb = new();

        sb.AppendLine("#nullable enable");
        sb.AppendLine("using DevoidEngine.Engine.Components;");
        sb.AppendLine("using DevoidEngine.Engine.Serialization;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("namespace DevoidEngine.Serialization.Generated");
        sb.AppendLine("{");
        sb.AppendLine("internal static class GeneratedComponentRegistry");
        sb.AppendLine("{");
        sb.AppendLine("[ModuleInitializer]");
        sb.AppendLine("    public static void RegisterAll()");
        sb.AppendLine("    {");

        foreach (var comp in components.Distinct(SymbolEqualityComparer.Default))
        {
            string name = comp.Name;
            string full = comp.ToDisplayString();

            sb.AppendLine(
                $"        DevoidEngine.Engine.Serialization.ComponentSerializationRegistry.Register<{full}>({name}Serializer.Serialize, {name}Serializer.Deserialize);");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("}");

        context.AddSource("GeneratedComponentRegistry.g.cs", sb.ToString());
    }
}