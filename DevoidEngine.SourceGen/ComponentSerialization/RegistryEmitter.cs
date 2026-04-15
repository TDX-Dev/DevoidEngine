using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DevoidEngine.SourceGen.ComponentSerialization
{
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
            sb.AppendLine("namespace DevoidEngine.Engine.Serialization.Generated");
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
                    $"        ComponentSerializationRegistry.Register(");
                sb.AppendLine(
                    $"            typeof({full}),");
                sb.AppendLine($"            new Func<Component, byte[]>(Serialize_{name}),");
                sb.AppendLine($"            new Func<byte[], Component>(Deserialize_{name}));");
            }

            sb.AppendLine("    }");

            // Generate wrapper methods
            foreach (var comp in components.Distinct(SymbolEqualityComparer.Default))
            {
                string name = comp.Name;
                string full = comp.ToDisplayString();

                sb.AppendLine($@"
    static byte[] Serialize_{name}(Component c)
    {{
        return {name}Serializer.Serialize(({full})c);
    }}

    static Component Deserialize_{name}(byte[] data)
    {{
        return {name}Serializer.Deserialize(data);
    }}
");
            }

            sb.AppendLine("}");
            sb.AppendLine("}");

            context.AddSource("GeneratedComponentRegistry.g.cs", sb.ToString());
        }
    }
}
