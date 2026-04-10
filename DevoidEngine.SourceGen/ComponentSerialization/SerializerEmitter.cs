using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevoidEngine.SourceGen.ComponentSerialization;

internal static class SerializerEmitter
{
    static readonly HashSet<string> AllowedTypes = new()
        {
            "DevoidEngine.Engine.Rendering.LightType",
            "DevoidEngine.Engine.Rendering.LightAttenuationType"
        };

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
                !HasDontSerializeAttribute(f) &&
                (
                    f.DeclaredAccessibility == Accessibility.Public ||
                    f.DeclaredAccessibility == Accessibility.Internal ||
                    HasSerializeFieldAttribute(f)
                ) &&
                f.Name != "gameObject" &&
                f.Type.ToDisplayString() != "DevoidEngine.Engine.Core.GameObject" &&
                (
                    IsAssetType(f.Type) ||
                    IsWhitelisted(f.Type) ||
                    (
                        !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.Core") &&
                        !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.Rendering") &&
                        !f.Type.ToDisplayString().StartsWith("DevoidEngine.Engine.UI")
                    )
                )
            ).ToArray();

        StringBuilder serializeBody = new();
        StringBuilder deserializeBody = new();

        serializeBody.AppendLine($"writer.WriteArrayHeader({fields.Length});");

        foreach (var field in fields)
        {
            string fieldName = field.Name;
            string type = field.Type.ToDisplayString();

            serializeBody.AppendLine($"// Serialize field: {fieldName}");

            // ---------------- ASSET REFERENCES ----------------

            if (IsAssetType(field.Type))
            {
                serializeBody.AppendLine($$"""
                try
                {
                    MessagePack.MessagePackSerializer.Serialize(
                        ref writer,
                        value.{{fieldName}}?.Guid ?? Guid.Empty,
                        MessagePack.MessagePackSerializerOptions.Standard);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Serialization] Failed to serialize asset field '{{fieldName}}' in {{componentName}}: " + e.Message);
                    writer.WriteNil();
                }
                """);

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
                        Console.WriteLine("[Serialization] Failed to deserialize asset field '{{fieldName}}' in {{componentName}}: " + e.Message);
                    }
                }
                """);
            }

            // ---------------- COMPONENT REFERENCES ----------------

            else if (IsComponentType(field.Type))
            {
                serializeBody.AppendLine($$"""
                try
                {
                    if (value.{{fieldName}} == null)
                    {
                        writer.WriteNil();
                    }
                    else
                    {
                        writer.WriteArrayHeader(2);

                        MessagePack.MessagePackSerializer.Serialize(
                            ref writer,
                            value.{{fieldName}}.gameObject.Id,
                            MessagePack.MessagePackSerializerOptions.Standard);

                        writer.Write(value.{{fieldName}}.GetType().AssemblyQualifiedName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Serialization] Failed to serialize component reference '{{fieldName}}' in {{componentName}}: " + e.Message);
                    writer.WriteNil();
                }
                """);

                deserializeBody.AppendLine($$"""
                if (!reader.End)
                {
                    try
                    {
                        if (reader.TryReadNil())
                        {
                            component.{{fieldName}} = null;
                        }
                        else
                        {
                            reader.ReadArrayHeader();

                            var goId =
                                MessagePack.MessagePackSerializer.Deserialize<Guid>(
                                    ref reader,
                                    MessagePack.MessagePackSerializerOptions.Standard);

                            string compType = reader.ReadString() ?? "";

                            GameObjectSerializer.RegisterComponentReference(
                                component,
                                (owner, value) =>
                                {
                                    (({{namespaceName}}.{{componentName}})owner).{{fieldName}} =
                                        ({{type}})value;
                                },
                                goId,
                                compType
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[Serialization] Failed to deserialize component reference '{{fieldName}}' in {{componentName}}: " + e.Message);
                    }
                }
                """);
            }

            // ---------------- PRIMITIVES ----------------

            else if (IsPrimitive(field.Type))
            {
                serializeBody.AppendLine($$"""
                try
                {
                    writer.Write(value.{{fieldName}});
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Serialization] Failed to serialize field '{{fieldName}}' in {{componentName}}: " + e.Message);
                    writer.WriteNil();
                }
                """);

                deserializeBody.AppendLine($$"""
                if (!reader.End)
                {
                    try
                    {
                        component.{{fieldName}} = reader.Read{{GetPrimitiveReader(field.Type)}}();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[Serialization] Failed to deserialize field '{{fieldName}}' in {{componentName}}: " + e.Message);
                    }
                }
                """);
            }

            // ---------------- OTHER TYPES ----------------

            else
            {
                serializeBody.AppendLine($$"""
                try
                {
                    MessagePack.MessagePackSerializer.Serialize(
                        ref writer,
                        value.{{fieldName}},
                        MessagePack.MessagePackSerializerOptions.Standard);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Serialization] Failed to serialize field '{{fieldName}}' in {{componentName}}: " + e.Message);
                    writer.WriteNil();
                }
                """);

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
                    catch (Exception e)
                    {
                        Console.WriteLine("[Serialization] Failed to deserialize field '{{fieldName}}' in {{componentName}}: " + e.Message);
                    }
                }
                """);
            }
        }

        deserializeBody.AppendLine("""
        while (!reader.End)
        {
            try { reader.Skip(); }
            catch { break; }
        }
        """);

        string source = $$"""
        #nullable enable
        using MessagePack;
        using System.Buffers;
        using DevoidEngine.Engine.AssetPipeline;
        using DevoidEngine.Engine.Serialization;

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

                    try { reader.ReadArrayHeader(); }
                    catch { return component; }

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
        return field.GetAttributes().Any(a => a.AttributeClass?.Name == "SerializeField");
    }

    private static bool HasDontSerializeAttribute(IFieldSymbol field)
    {
        return field.GetAttributes().Any(a => a.AttributeClass?.Name == "DontSerialize");
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

    private static bool IsComponentType(ITypeSymbol type)
    {
        while (type != null)
        {
            if (type.ToDisplayString() == "DevoidEngine.Engine.Components.Component")
                return true;

            type = type.BaseType;
        }

        return false;
    }

    private static bool IsWhitelisted(ITypeSymbol type)
    {
        return AllowedTypes.Contains(type.ToDisplayString());
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
}