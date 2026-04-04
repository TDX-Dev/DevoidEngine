using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DevoidEngine.SourceGen.ComponentSerialization;

internal static class SerializerEmitter
{
    public static void Emit(SourceProductionContext context, INamedTypeSymbol component)
    {
        string namespaceName = component.ContainingNamespace.ToDisplayString();
        string componentName = component.Name;
        string serializerName = componentName + "Serializer";

        var fields = component
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f =>
                !f.IsStatic &&
                f.DeclaredAccessibility == Accessibility.Public &&
                f.Name != "gameObject" &&
                !ContainsGameObject(f.Type) &&
                !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.Core") &&
                !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.Rendering") &&
                !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.UI"));

        StringBuilder serializeBody = new();
        StringBuilder deserializeBody = new();

        int fieldCount = fields.Count();

        serializeBody.AppendLine($"writer.WriteArrayHeader({fieldCount});");

        foreach (var field in fields)
        {
            string fieldName = field.Name;
            string type = field.Type.ToDisplayString();

            serializeBody.AppendLine($"// Serialize field: {fieldName} ({type})");

            if (IsPrimitive(field.Type))
            {
                serializeBody.AppendLine($"writer.Write(value.{fieldName});");

                deserializeBody.AppendLine($"// Deserialize field: {fieldName} ({type})");
                deserializeBody.AppendLine($"component.{fieldName} = reader.Read{GetPrimitiveReader(type)}();");
            }
            else
            {
                serializeBody.AppendLine($"MessagePack.MessagePackSerializer.Serialize(ref writer, value.{fieldName}, MessagePack.MessagePackSerializerOptions.Standard);");

                deserializeBody.AppendLine($"// Deserialize field: {fieldName} ({type})");
                deserializeBody.AppendLine($"component.{fieldName} = MessagePack.MessagePackSerializer.Deserialize<{type}>(ref reader, MessagePack.MessagePackSerializerOptions.Standard);");
            }
        }

        string source = $$"""
            #nullable enable
            using MessagePack;
            using System.Buffers;

            namespace DevoidEngine.Serialization.Generated
            {
                internal static class {{serializerName}}
                {
                    public static byte[] Serialize({{namespaceName}}.{{componentName}} value)
                    {
                        var buffer = new ArrayBufferWriter<byte>();
                        var writer = new MessagePackWriter(buffer);

                        {{serializeBody}}

                        writer.Flush();
                        return buffer.WrittenSpan.ToArray();
                    }

                    public static {{namespaceName}}.{{componentName}} Deserialize(byte[] data)
                    {
                        var reader = new MessagePackReader(data);

                        int count = reader.ReadArrayHeader();

                        var component = new {{namespaceName}}.{{componentName}}();

                        {{deserializeBody}}

                        return component;
                    }
                }
            }
        """;

        context.AddSource($"{componentName}.ComponentSerializer.g.cs", source);
    }

    private static bool ContainsGameObject(ITypeSymbol type)
    {
        if (type.ToDisplayString() == "DevoidEngine.Engine.Core.GameObject")
            return true;

        if (type is INamedTypeSymbol named)
        {
            foreach (var member in named.GetMembers().OfType<IFieldSymbol>())
            {
                if (member.Type.ToDisplayString() == "DevoidEngine.Engine.Core.GameObject")
                    return true;
            }
        }

        return false;
    }

    private static bool IsPrimitive(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Int32 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Boolean => true,
            SpecialType.System_Double => true,
            SpecialType.System_String => true,
            _ => false
        };
    }

    private static string GetPrimitiveReader(string type)
    {
        return type switch
        {
            "int" => "Int32",
            "float" => "Single",
            "bool" => "Boolean",
            "double" => "Double",
            "string" => "String",
            _ => "Object"
        };
    }
}