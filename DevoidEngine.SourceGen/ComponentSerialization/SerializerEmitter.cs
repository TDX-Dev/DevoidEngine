using Microsoft.CodeAnalysis;
using System;
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
                (f.DeclaredAccessibility == Accessibility.Public || HasSerializeFieldAttribute(f)) &&
                f.Name != "gameObject" &&
                f.Type.ToDisplayString() != "DevoidEngine.Engine.Core.GameObject" &&
                !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.Core") &&
                !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.Rendering") &&
                !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.UI"))
            .ToArray();

        StringBuilder serializeBody = new();
        StringBuilder deserializeBody = new();

        int fieldCount = fields.Length;

        serializeBody.AppendLine($"writer.WriteArrayHeader({fieldCount});");

        foreach (var field in fields)
        {
            string fieldName = field.Name;
            string type = field.Type.ToDisplayString();

            serializeBody.AppendLine($"// Serialize field: {fieldName} ({type})");

            if (IsAssetType(field.Type))
            {
                //serializeBody.AppendLine(
                //    $"MessagePack.MessagePackSerializer.Serialize(ref writer, value.{fieldName}?.Guid ?? Guid.Empty, MessagePack.MessagePackSerializerOptions.Standard);");

                serializeBody.AppendLine(
                    "try\n" +
                    "{\n" +
                    $"    MessagePack.MessagePackSerializer.Serialize(ref writer, value.{fieldName}?.Guid ?? Guid.Empty, MessagePack.MessagePackSerializerOptions.Standard);\n" +
                    "}\n" +
                    "catch (Exception e)\n" +
                    "{\n" +
                    $"    Console.WriteLine(\"[Serialization] Failed to serialize asset field '{fieldName}' in {componentName}: \" + e.Message);\n" +
                    "    writer.WriteNil();\n" +
                "}");


                //deserializeBody.AppendLine(
                //    $"var guid_{fieldName} = MessagePack.MessagePackSerializer.Deserialize<Guid>(ref reader, MessagePack.MessagePackSerializerOptions.Standard);");

                //deserializeBody.AppendLine(
                //    $"component.{fieldName} = guid_{fieldName} == Guid.Empty ? default : AssetManager.Load<{type}>(guid_{fieldName});");

                deserializeBody.AppendLine($$"""
                    if (!reader.End)
                    {
                        try
                        {
                            var guid_{{fieldName}} =
                                MessagePack.MessagePackSerializer.Deserialize<Guid>(
                                    ref reader,
                                    MessagePack.MessagePackSerializerOptions.Standard);

                            component.{{fieldName}} =
                                guid_{{fieldName}} == Guid.Empty
                                    ? default
                                    : AssetManager.Load<{{type}}>(guid_{{fieldName}});
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[Serialization] Failed to deserialize asset field '{{fieldName}}' in {{componentName}}: " + e.Message);
                            component.{{fieldName}} = default;
                        }
                    }
                """);
            } 
            else if (IsPrimitive(field.Type))
            {
                //serializeBody.AppendLine($"writer.Write(value.{fieldName});");

                serializeBody.AppendLine(
                    "try\n" +
                    "{\n" +
                    $"    writer.Write(value.{fieldName});\n" +
                    "}\n" +
                    "catch (Exception e)\n" +
                    "{\n" +
                    $"    Console.WriteLine(\"[Serialization] Failed to serialize field '{fieldName}' in {componentName}: \" + e.Message);\n" +
                    "    writer.WriteNil();\n" +
                    "}");

                deserializeBody.AppendLine($"// Deserialize field: {fieldName} ({type})");
                //deserializeBody.AppendLine($"component.{fieldName} = reader.Read{GetPrimitiveReader(type)}();");
                deserializeBody.AppendLine(
                    $"if (!reader.End) {{ try {{ component.{fieldName} = reader.Read{GetPrimitiveReader(field.Type)}(); }} catch (Exception e) {{ Console.WriteLine(\"[Serialization] Failed to deserialize field '{fieldName}' in {componentName}: \" + e.Message); }} }}"
                );
            }
            else
            {
                //serializeBody.AppendLine($"MessagePack.MessagePackSerializer.Serialize(ref writer, value.{fieldName}, MessagePack.MessagePackSerializerOptions.Standard);");
                serializeBody.AppendLine(
                    "try\n" +
                    "{\n" +
                    $"    MessagePack.MessagePackSerializer.Serialize(ref writer, value.{fieldName}, MessagePack.MessagePackSerializerOptions.Standard);\n" +
                    "}\n" +
                    "catch (Exception e)\n" +
                    "{\n" +
                    $"    Console.WriteLine(\"[Serialization] Failed to serialize field '{fieldName}' in {componentName}: \" + e.Message);\n" +
                    "    writer.WriteNil();\n" +
                    "}");

                deserializeBody.AppendLine($"// Deserialize field: {fieldName} ({type})");
                //deserializeBody.AppendLine($"component.{fieldName} = MessagePack.MessagePackSerializer.Deserialize<{type}>(ref reader, MessagePack.MessagePackSerializerOptions.Standard);");
                deserializeBody.AppendLine($$"""
                    if (!reader.End)
                    {
                        try
                        {
                            component.{{fieldName}} =
                                MessagePack.MessagePackSerializer.Deserialize<{{type}}>(
                                    ref reader,
                                    MessagePack.MessagePackSerializerOptions.Standard);
                        }
                        catch { }
                    }
                """);
            }
        }

        deserializeBody.AppendLine("""
            while (!reader.End)
            {
                try
                {
                    reader.Skip();
                }
                catch
                {
                    break;
                }
            }
            """);

        string source = $$"""
            #nullable enable
            using MessagePack;
            using System.Buffers;
            using DevoidEngine.Engine.AssetPipeline;

            namespace DevoidEngine.Engine.Serialization.Generated
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

                        var component = new {{namespaceName}}.{{componentName}}();

                        int count;
                        try
                        {
                            count = reader.ReadArrayHeader();
                        }
                        catch
                        {
                            return component;
                        }

                        {{deserializeBody}}

                        return component;
                    }
                }
            }
        """;

        context.AddSource($"{componentName}.ComponentSerializer.g.cs", source);
    }

    private static bool HasSerializeFieldAttribute(IFieldSymbol field)
    {
        foreach (var attr in field.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "SerializeFieldAttribute")
                return true;
        }

        return false;
    }

    private static bool IsAssetType(ITypeSymbol type)
    {
        while (type != null)
        {
            if (type.Name == "AssetType")
                return true;

            type = type.BaseType;
        }

        return false;
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

    private static string GetPrimitiveReader(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Int32 => "Int32",
            SpecialType.System_Single => "Single",
            SpecialType.System_Boolean => "Boolean",
            SpecialType.System_Double => "Double",
            SpecialType.System_String => "String",
            _ => throw new Exception("Unsupported primitive")
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