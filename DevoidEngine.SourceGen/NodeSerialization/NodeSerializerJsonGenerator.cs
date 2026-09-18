using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevoidEngine.SourceGen.NodeSerialization
{

    public class NodeSerializerJsonGenerator
    {

        private static readonly HashSet<string> SpecialTypes =
        [
            "System.Guid",
            "System.Numerics.Vector2",
            "System.Numerics.Vector3",
            "System.Numerics.Vector4",
            "System.Numerics.Quaternion",
            "System.Numerics.Matrix4x4"
        ];

        private static string GetSafeTypeName(ITypeSymbol type)
        {
            return type.Name
                .Replace(".", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace(",", "_");
        }
        private static bool IsList(INamedTypeSymbol type)
        {
            return type.IsGenericType &&
                   type.ConstructedFrom.ToDisplayString() ==
                   "System.Collections.Generic.List<T>";
        }
        private static IEnumerable<ISymbol> GetSerializableMembers(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            {
                foreach (ISymbol member in current.GetMembers())
                {
                    if (member.IsStatic)
                        continue;

                    if (member.DeclaredAccessibility != Accessibility.Public)
                        continue;

                    if (member is IFieldSymbol)
                    {
                        yield return member;
                    }
                    else if (member is IPropertySymbol property &&
                             !property.IsIndexer &&
                             property.GetMethod is not null &&
                             property.SetMethod is not null)
                    {
                        yield return property;
                    }
                }
            }
        }
        private static bool HasAttribute(INamedTypeSymbol type, string attributeName)
        {
            foreach (AttributeData attribute in type.GetAttributes())
            {
                if (attribute.AttributeClass?.Name == attributeName)
                    return true;
            }

            return false;
        }
        public static void Generate(SourceProductionContext context, INamedTypeSymbol type)
        {
            ISymbol[] members = [.. GetSerializableMembers(type)];

            StringBuilder sb = new();

            sb.AppendLine("using System.Text.Json;");
            sb.AppendLine("using DevoidEngine.Serialization;");
            sb.AppendLine();
            sb.AppendLine("namespace DevoidEngine.Generated;");
            sb.AppendLine();
            sb.AppendLine($"internal static class {type.Name}Serializer");
            sb.AppendLine("{");
            sb.AppendLine($"    public static void Write(Utf8JsonWriter writer, {type.ToDisplayString()} value)");
            sb.AppendLine("    {");
            sb.AppendLine("        writer.WriteStartObject();");

            foreach (ISymbol member in members)
            {
                ITypeSymbol memberType;

                if (member is IFieldSymbol field)
                {
                    memberType = field.Type;
                }
                else if (member is IPropertySymbol property)
                {
                    memberType = property.Type;
                }
                else
                {
                    continue;
                }
                string expression = $"value.{member.Name}";

                if (!GenerateWriteProperty(sb, member.Name, memberType, expression))
                    continue;
            }

            sb.AppendLine("        writer.WriteEndObject();");
            sb.AppendLine("    }");
            sb.AppendLine($"    public static {type.ToDisplayString()} Read(ref Utf8JsonReader reader)");
            sb.AppendLine("    {");
            sb.AppendLine($"        {type.ToDisplayString()} value = new();");
            sb.AppendLine();
            sb.AppendLine("        if (reader.TokenType != JsonTokenType.StartObject)");
            sb.AppendLine("            throw new JsonException();");
            sb.AppendLine();
            sb.AppendLine("        while (reader.Read())");
            sb.AppendLine("        {");
            sb.AppendLine("            if (reader.TokenType == JsonTokenType.EndObject)");
            sb.AppendLine("                break;");
            sb.AppendLine();
            sb.AppendLine("            if (reader.TokenType != JsonTokenType.PropertyName)");
            sb.AppendLine("                throw new JsonException();");
            sb.AppendLine();
            sb.AppendLine("            string propertyName = reader.GetString()!;");
            sb.AppendLine();
            sb.AppendLine("            if (!reader.Read())");
            sb.AppendLine("                throw new JsonException();");
            sb.AppendLine();
            sb.AppendLine("            switch (propertyName)");
            sb.AppendLine("            {");

            foreach (ISymbol member in members)
            {
                ITypeSymbol memberType;

                if (member is IFieldSymbol field)
                {
                    memberType = field.Type;
                }
                else if (member is IPropertySymbol property)
                {
                    memberType = property.Type;
                }
                else
                {
                    continue;
                }

                sb.AppendLine($"                case \"{member.Name}\":");

                if (GenerateRead(sb, memberType, $"value.{member.Name}"))
                {
                    sb.AppendLine("                    break;");
                }
                else
                {
                    sb.AppendLine("                    reader.Skip();");
                    sb.AppendLine("                    break;");
                }
            }
            sb.AppendLine("                default:");
            sb.AppendLine("                    reader.Skip();");
            sb.AppendLine("                    break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return value;");
            sb.AppendLine("    }");
            GenerateReadHelpers(sb, type);
            sb.AppendLine("}");

            context.AddSource(
                $"{type.Name}.Serializer.g.cs",
                sb.ToString());
        }
        private static bool GenerateWrite(StringBuilder sb, ITypeSymbol type, string expression)
        {
            string typeName = type.ToDisplayString();

            if (SpecialTypes.Contains(typeName))
            {
                sb.AppendLine($"\t\tValueWriter.Write(writer, {expression});");

                return true;
            }


            switch (type.SpecialType)
            {
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Int16:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_UInt16:
                case SpecialType.System_SByte:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                    sb.AppendLine(
                        $"\t\twriter.WriteNumberValue({expression});");
                    return true;

                case SpecialType.System_Boolean:
                    sb.AppendLine(
                        $"\t\twriter.WriteBooleanValue({expression});");
                    return true;

                case SpecialType.System_String:
                    sb.AppendLine(
                        $"\t\twriter.WriteStringValue({expression});");
                    return true;

                case SpecialType.System_Char:
                    sb.AppendLine(
                        $"\t\twriter.WriteStringValue({expression}.ToString());");
                    return true;
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                sb.AppendLine($"\t\twriter.WriteStringValue({expression}.ToString());");

                return true;
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                return GenerateArrayWrite(sb, arrayType.ElementType, expression);
            }
            if (type is INamedTypeSymbol listType && IsList(listType))
            {
                ITypeSymbol elementType = listType.TypeArguments[0];
                return GenerateArrayWrite(sb, elementType, expression);
            }

            if (type is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Struct)
            {
                GenerateStructWrite(sb, namedType, expression);

                return true;
            }

            if (type is INamedTypeSymbol namedClassType && namedClassType.TypeKind == TypeKind.Class && HasAttribute(namedClassType, "DevoidDataClass"))
            {
                GenerateObjectWrite(sb, namedClassType, expression);
                return true;
            }

            return false;
        }
        private static void GenerateObjectWrite(StringBuilder sb, INamedTypeSymbol type, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");

            foreach (ISymbol member in GetSerializableMembers(type))
            {
                ITypeSymbol memberType;

                if (member is IFieldSymbol field)
                {
                    memberType = field.Type;
                }
                else if (member is IPropertySymbol property)
                {
                    memberType = property.Type;
                }
                else
                {
                    continue;
                }

                if (!GenerateWriteProperty(sb, member.Name, memberType, $"{expression}.{member.Name}"))
                {
                    continue;
                }
            }

            sb.AppendLine("\t\twriter.WriteEndObject();");
        }
        private static void GenerateStructWrite(StringBuilder sb, INamedTypeSymbol type, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");

            foreach (ISymbol member in GetSerializableMembers(type))
            {
                ITypeSymbol memberType;

                if (member is IFieldSymbol field)
                {
                    memberType = field.Type;
                }
                else if (member is IPropertySymbol property)
                {
                    memberType = property.Type;
                }
                else
                {
                    continue;
                }

                GenerateWriteProperty(
                    sb,
                    member.Name,
                    memberType,
                    $"{expression}.{member.Name}");
            }

            sb.AppendLine("\t\twriter.WriteEndObject();");
        }
        private static bool GenerateWriteProperty(StringBuilder sb, string name, ITypeSymbol type, string expression)
        {
            int startLength = sb.Length;

            sb.AppendLine($"        writer.WritePropertyName(\"{name}\");");

            if (GenerateWrite(sb, type, expression))
                return true;

            sb.Length = startLength;
            return false;
        }
        private static bool GenerateArrayWrite(StringBuilder sb, ITypeSymbol elementType, string expression)
        {
            int startLength = sb.Length;
            sb.AppendLine("\t\twriter.WriteStartArray();");

            sb.AppendLine(
                $"\t\tforeach (var item in {expression})");
            sb.AppendLine("\t\t{");

            if (!GenerateWrite(sb, elementType, "item"))
            {
                sb.Length = startLength;
                return false;
            }

            sb.AppendLine("\t\t}");

            sb.AppendLine("\t\twriter.WriteEndArray();");
            return true;
        }
        private static void GenerateGuidWrite(StringBuilder sb, string expression)
        {
            sb.AppendLine($"\t\twriter.WriteStringValue({expression}.ToString());");
        }
        private static void GenerateVector2Write(StringBuilder sb, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");
            sb.AppendLine($"\t\twriter.WriteNumber(\"X\", {expression}.X);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Y\", {expression}.Y);");
            sb.AppendLine("\t\twriter.WriteEndObject();");
        }
        private static void GenerateVector3Write(StringBuilder sb, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");
            sb.AppendLine($"\t\twriter.WriteNumber(\"X\", {expression}.X);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Y\", {expression}.Y);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Z\", {expression}.Z);");
            sb.AppendLine("\t\twriter.WriteEndObject();");
        }
        private static void GenerateVector4Write(StringBuilder sb, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");
            sb.AppendLine($"\t\twriter.WriteNumber(\"X\", {expression}.X);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Y\", {expression}.Y);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Z\", {expression}.Z);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"W\", {expression}.W);");
            sb.AppendLine("\t\twriter.WriteEndObject();");
        }
        private static void GenerateQuaternionWrite(StringBuilder sb, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");
            sb.AppendLine($"\t\twriter.WriteNumber(\"X\", {expression}.X);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Y\", {expression}.Y);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"Z\", {expression}.Z);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"W\", {expression}.W);");
            sb.AppendLine("\t\twriter.WriteEndObject();");
        }
        private static void GenerateMatrix4x4Write(StringBuilder sb, string expression)
        {
            sb.AppendLine("\t\twriter.WriteStartObject();");

            sb.AppendLine($"\t\twriter.WriteNumber(\"M11\", {expression}.M11);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M12\", {expression}.M12);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M13\", {expression}.M13);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M14\", {expression}.M14);");

            sb.AppendLine($"\t\twriter.WriteNumber(\"M21\", {expression}.M21);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M22\", {expression}.M22);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M23\", {expression}.M23);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M24\", {expression}.M24);");

            sb.AppendLine($"\t\twriter.WriteNumber(\"M31\", {expression}.M31);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M32\", {expression}.M32);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M33\", {expression}.M33);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M34\", {expression}.M34);");

            sb.AppendLine($"\t\twriter.WriteNumber(\"M41\", {expression}.M41);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M42\", {expression}.M42);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M43\", {expression}.M43);");
            sb.AppendLine($"\t\twriter.WriteNumber(\"M44\", {expression}.M44);");

            sb.AppendLine("\t\twriter.WriteEndObject();");
        }

        private static bool GenerateRead(StringBuilder sb, ITypeSymbol type, string target)
        {
            string typeName = type.ToDisplayString();

            if (SpecialTypes.Contains(typeName))
            {
                sb.AppendLine($"\t\t\t\t\t{target} = ValueReader.Read{GetSafeTypeName(type)}(ref reader);");

                return true;
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Int32:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetInt32();");
                    return true;

                case SpecialType.System_Int64:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetInt64();");
                    return true;

                case SpecialType.System_Int16:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetInt16();");
                    return true;

                case SpecialType.System_Byte:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetByte();");
                    return true;

                case SpecialType.System_UInt32:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetUInt32();");
                    return true;

                case SpecialType.System_UInt64:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetUInt64();");
                    return true;

                case SpecialType.System_UInt16:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetUInt16();");
                    return true;

                case SpecialType.System_SByte:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetSByte();");
                    return true;

                case SpecialType.System_Single:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetSingle();");
                    return true;

                case SpecialType.System_Double:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetDouble();");
                    return true;

                case SpecialType.System_Decimal:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetDecimal();");
                    return true;

                case SpecialType.System_Boolean:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetBoolean();");
                    return true;

                case SpecialType.System_String:
                    sb.AppendLine($"\t\t\t\t\t{target} = reader.GetString()!;");
                    return true;

                case SpecialType.System_Char:
                    sb.AppendLine(
                        $"\t\t\t\t\t{target} = reader.GetString()![0];");
                    return true;
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                string enumType = type.ToDisplayString();

                sb.AppendLine($"\t\t\t\t\t{target} = System.Enum.Parse<{enumType}>(reader.GetString()!);");

                return true;
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                return GenerateArrayRead(sb, arrayType.ElementType, target);
            }

            if (type is INamedTypeSymbol listType && IsList(listType))
            {
                return GenerateListRead(sb, listType.TypeArguments[0], target);
            }

            if (type is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Struct && HasAttribute(namedType, "DevoidDataClass"))
            {
                sb.AppendLine($"\t\t\t\t\t{target} = Read{GetSafeTypeName(namedType)}(ref reader);");

                return true;
            }

            if (type is INamedTypeSymbol namedClassType && namedClassType.TypeKind == TypeKind.Class && HasAttribute(namedClassType, "DevoidDataClass"))
            {
                sb.AppendLine($"\t\t\t\t\t{target} = Read{GetSafeTypeName(namedClassType)}(ref reader);");

                return true;
            }

            return false;
        }
        private static bool GenerateArrayRead(StringBuilder sb, ITypeSymbol elementType, string target)
        {
            int startLength = sb.Length;

            string elementTypeName = elementType.ToDisplayString();

            sb.AppendLine(
                "\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartArray)");

            sb.AppendLine(
                "\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine(
                $"\t\t\t\t\tvar list = new List<{elementTypeName}>();");

            sb.AppendLine();

            sb.AppendLine(
                "\t\t\t\t\twhile (reader.Read() && reader.TokenType != JsonTokenType.EndArray)");

            sb.AppendLine("\t\t\t\t\t{");

            sb.AppendLine(
                $"\t\t\t\t\t\t{elementTypeName} item = default!;");

            if (!GenerateRead(sb, elementType, "item"))
            {
                sb.Length = startLength;
                return false;
            }

            sb.AppendLine(
                "\t\t\t\t\t\tlist.Add(item);");

            sb.AppendLine("\t\t\t\t\t}");

            sb.AppendLine();

            sb.AppendLine(
                $"\t\t\t\t\t{target} = list.ToArray();");

            return true;
        }
        private static bool GenerateListRead(StringBuilder sb, ITypeSymbol elementType, string target)
        {
            int startLength = sb.Length;

            string elementTypeName = elementType.ToDisplayString();

            sb.AppendLine(
                "\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartArray)");

            sb.AppendLine(
                "\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine(
                $"\t\t\t\t\t{target} = new List<{elementTypeName}>();");

            sb.AppendLine();

            sb.AppendLine(
                "\t\t\t\t\twhile (reader.Read() && reader.TokenType != JsonTokenType.EndArray)");

            sb.AppendLine("\t\t\t\t\t{");

            sb.AppendLine(
                $"\t\t\t\t\t\t{elementTypeName} item = default!;");

            if (!GenerateRead(sb, elementType, "item"))
            {
                sb.Length = startLength;
                return false;
            }

            sb.AppendLine(
                $"\t\t\t\t\t\t{target}.Add(item);");

            sb.AppendLine("\t\t\t\t\t}");

            return true;
        }
        private static void GenerateGuidRead(StringBuilder sb, string target)
        {
            sb.AppendLine($"\t\t\t\t\t{target} = Guid.Parse(reader.GetString()!);");
        }
        private static void GenerateVector2Read(StringBuilder sb, string target)
        {
            sb.AppendLine("\t\t\t\t\tvar result = new System.Numerics.Vector2();");

            sb.AppendLine("\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartObject)");

            sb.AppendLine("\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\twhile (reader.Read())");

            sb.AppendLine("\t\t\t\t\t{");

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType == JsonTokenType.EndObject)");

            sb.AppendLine("\t\t\t\t\t\t\tbreak;");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType != JsonTokenType.PropertyName)");

            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tstring propertyName = reader.GetString()!;");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (!reader.Read())");

            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tswitch (propertyName)");

            sb.AppendLine("\t\t\t\t\t\t{");

            sb.AppendLine("\t\t\t\t\t\t\tcase \"X\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tresult.X = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t\tcase \"Y\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tresult.Y = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t\tdefault:");
            sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t}");

            sb.AppendLine("\t\t\t\t\t}");

            sb.AppendLine();

            sb.AppendLine($"\t\t\t\t\t{target} = result;");
        }
        private static void GenerateVector3Read(StringBuilder sb, string target)
        {
            sb.AppendLine("\t\t\t\t\tvar result = new System.Numerics.Vector3();");

            sb.AppendLine("\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartObject)");

            sb.AppendLine("\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\twhile (reader.Read())");

            sb.AppendLine("\t\t\t\t\t{");

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType == JsonTokenType.EndObject)");

            sb.AppendLine("\t\t\t\t\t\t\tbreak;");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType != JsonTokenType.PropertyName)");

            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tstring propertyName = reader.GetString()!;");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (!reader.Read())");

            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tswitch (propertyName)");

            sb.AppendLine("\t\t\t\t\t\t{");

            sb.AppendLine("\t\t\t\t\t\t\tcase \"X\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tresult.X = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t\tcase \"Y\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tresult.Y = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t\tcase \"Z\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tresult.Z = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t\tdefault:");
            sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t}");

            sb.AppendLine("\t\t\t\t\t}");

            sb.AppendLine();

            sb.AppendLine($"\t\t\t\t\t{target} = result;");
        }
        private static bool GenerateStructRead(StringBuilder sb, INamedTypeSymbol type, string target)
        {
            string typeName = type.ToDisplayString();

            sb.AppendLine($"\t\t\t\t\t{typeName} result = new();");

            sb.AppendLine();

            GenerateReadObjectBody(sb, type, "result");

            sb.AppendLine();

            sb.AppendLine($"\t\t\t\t\t{target} = result;");

            return true;
        }
        private static void GenerateReadObjectBody(StringBuilder sb, INamedTypeSymbol type, string target)
        {
            sb.AppendLine("\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartObject)");

            sb.AppendLine("\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\twhile (reader.Read())");

            sb.AppendLine("\t\t\t\t\t{");

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType == JsonTokenType.EndObject)");

            sb.AppendLine("\t\t\t\t\t\t\tbreak;");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType != JsonTokenType.PropertyName)");

            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tstring propertyName = reader.GetString()!;");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (!reader.Read())");

            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");

            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tswitch (propertyName)");

            sb.AppendLine("\t\t\t\t\t\t{");

            foreach (ISymbol member in GetSerializableMembers(type))
            {
                ITypeSymbol memberType = member switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    _ => null!
                };

                if (memberType is null)
                    continue;

                sb.AppendLine($"\t\t\t\t\t\t\tcase \"{member.Name}\":");

                if (GenerateRead(sb, memberType, $"{target}.{member.Name}"))
                {
                    sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
                }
                else
                {
                    sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");

                    sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
                }
            }

            sb.AppendLine("\t\t\t\t\t\t\tdefault:");
            sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");

            sb.AppendLine("\t\t\t\t\t\t}");

            sb.AppendLine("\t\t\t\t\t}");
        }
        private static bool GenerateObjectRead(StringBuilder sb, INamedTypeSymbol type, string target)
        {
            string typeName = type.ToDisplayString();

            sb.AppendLine($"\t\t\t\t\t{typeName} result = new();");

            sb.AppendLine();

            GenerateReadObjectBody(sb, type, "result");

            sb.AppendLine();

            sb.AppendLine($"\t\t\t\t\t{target} = result;");

            return true;
        }
        private static void GenerateVector4Read(StringBuilder sb, string target)
        {
            sb.AppendLine("\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartObject)");
            sb.AppendLine("\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\tfloat x = 0;");
            sb.AppendLine("\t\t\t\t\tfloat y = 0;");
            sb.AppendLine("\t\t\t\t\tfloat z = 0;");
            sb.AppendLine("\t\t\t\t\tfloat w = 0;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\twhile (reader.Read())");
            sb.AppendLine("\t\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType == JsonTokenType.EndObject)");
            sb.AppendLine("\t\t\t\t\t\t\tbreak;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType != JsonTokenType.PropertyName)");
            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tstring propertyName = reader.GetString()!;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (!reader.Read())");
            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tswitch (propertyName)");
            sb.AppendLine("\t\t\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"X\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tx = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"Y\":");
            sb.AppendLine("\t\t\t\t\t\t\t\ty = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"Z\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tz = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"W\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tw = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tdefault:");
            sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t}");
            sb.AppendLine("\t\t\t\t\t}");
            sb.AppendLine();

            sb.AppendLine($"\t\t\t\t\t{target} = new System.Numerics.Vector4(x, y, z, w);");
        }
        private static void GenerateQuaternionRead(StringBuilder sb, string target)
        {
            sb.AppendLine("\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartObject)");
            sb.AppendLine("\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\tfloat x = 0;");
            sb.AppendLine("\t\t\t\t\tfloat y = 0;");
            sb.AppendLine("\t\t\t\t\tfloat z = 0;");
            sb.AppendLine("\t\t\t\t\tfloat w = 1;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\twhile (reader.Read())");
            sb.AppendLine("\t\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType == JsonTokenType.EndObject)");
            sb.AppendLine("\t\t\t\t\t\t\tbreak;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType != JsonTokenType.PropertyName)");
            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tstring propertyName = reader.GetString()!;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (!reader.Read())");
            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tswitch (propertyName)");
            sb.AppendLine("\t\t\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"X\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tx = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"Y\":");
            sb.AppendLine("\t\t\t\t\t\t\t\ty = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"Z\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tz = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tcase \"W\":");
            sb.AppendLine("\t\t\t\t\t\t\t\tw = reader.GetSingle();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t\tdefault:");
            sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t}");
            sb.AppendLine("\t\t\t\t\t}");
            sb.AppendLine();

            sb.AppendLine($"\t\t\t\t\t{target} = new System.Numerics.Quaternion(x, y, z, w);");
        }
        private static void GenerateMatrix4x4Read(StringBuilder sb, string target)
        {
            sb.AppendLine("\t\t\t\t\tif (reader.TokenType != JsonTokenType.StartObject)");
            sb.AppendLine("\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\tfloat m11 = 0, m12 = 0, m13 = 0, m14 = 0;");
            sb.AppendLine("\t\t\t\t\tfloat m21 = 0, m22 = 0, m23 = 0, m24 = 0;");
            sb.AppendLine("\t\t\t\t\tfloat m31 = 0, m32 = 0, m33 = 0, m34 = 0;");
            sb.AppendLine("\t\t\t\t\tfloat m41 = 0, m42 = 0, m43 = 0, m44 = 0;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\twhile (reader.Read())");
            sb.AppendLine("\t\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType == JsonTokenType.EndObject)");
            sb.AppendLine("\t\t\t\t\t\t\tbreak;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (reader.TokenType != JsonTokenType.PropertyName)");
            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tstring propertyName = reader.GetString()!;");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tif (!reader.Read())");
            sb.AppendLine("\t\t\t\t\t\t\tthrow new JsonException();");
            sb.AppendLine();

            sb.AppendLine("\t\t\t\t\t\tswitch (propertyName)");
            sb.AppendLine("\t\t\t\t\t\t{");

            string[] fields =
            [
                "M11", "M12", "M13", "M14",
                "M21", "M22", "M23", "M24",
                "M31", "M32", "M33", "M34",
                "M41", "M42", "M43", "M44"
            ];

            foreach (string field in fields)
            {
                string local = field.ToLowerInvariant();

                sb.AppendLine($"\t\t\t\t\t\t\tcase \"{field}\":");
                sb.AppendLine($"\t\t\t\t\t\t\t\t{local} = reader.GetSingle();");
                sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            }

            sb.AppendLine("\t\t\t\t\t\t\tdefault:");
            sb.AppendLine("\t\t\t\t\t\t\t\treader.Skip();");
            sb.AppendLine("\t\t\t\t\t\t\t\tbreak;");
            sb.AppendLine("\t\t\t\t\t\t}");
            sb.AppendLine("\t\t\t\t\t}");
            sb.AppendLine();

            sb.AppendLine(
                $"\t\t\t\t\t{target} = new System.Numerics.Matrix4x4(" +
                "m11, m12, m13, m14, " +
                "m21, m22, m23, m24, " +
                "m31, m32, m33, m34, " +
                "m41, m42, m43, m44);");
        }
        private static void GenerateReadHelpers(StringBuilder sb, INamedTypeSymbol rootType)
        {
            HashSet<INamedTypeSymbol> generated =
                new(SymbolEqualityComparer.Default);

            foreach (ISymbol member in GetSerializableMembers(rootType))
            {
                ITypeSymbol? memberType = member switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    _ => null
                };

                if (memberType is not null)
                    GenerateNestedReadHelpers(sb, memberType, generated);
            }
        }
        private static void GenerateDataClassReadMethod(StringBuilder sb, INamedTypeSymbol type)
        {
            string typeName = type.ToDisplayString();
            string methodName = $"Read{GetSafeTypeName(type)}";

            sb.AppendLine();
            sb.AppendLine(
                $"    private static {typeName} {methodName}(ref Utf8JsonReader reader)");
            sb.AppendLine("    {");
            sb.AppendLine($"        {typeName} result = new();");
            sb.AppendLine();

            sb.AppendLine(
                "        if (reader.TokenType != JsonTokenType.StartObject)");
            sb.AppendLine("            throw new JsonException();");
            sb.AppendLine();

            sb.AppendLine("        while (reader.Read())");
            sb.AppendLine("        {");

            sb.AppendLine(
                "            if (reader.TokenType == JsonTokenType.EndObject)");
            sb.AppendLine("                break;");
            sb.AppendLine();

            sb.AppendLine(
                "            if (reader.TokenType != JsonTokenType.PropertyName)");
            sb.AppendLine("                throw new JsonException();");
            sb.AppendLine();

            sb.AppendLine(
                "            string propertyName = reader.GetString()!;");
            sb.AppendLine();

            sb.AppendLine("            if (!reader.Read())");
            sb.AppendLine("                throw new JsonException();");
            sb.AppendLine();

            sb.AppendLine("            switch (propertyName)");
            sb.AppendLine("            {");

            foreach (ISymbol member in GetSerializableMembers(type))
            {
                ITypeSymbol? memberType = member switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    _ => null
                };

                if (memberType is null)
                    continue;

                sb.AppendLine(
                    $"                case \"{member.Name}\":");

                if (GenerateRead(
                    sb,
                    memberType,
                    $"result.{member.Name}"))
                {
                    sb.AppendLine(
                        "                    break;");
                }
                else
                {
                    sb.AppendLine(
                        "                    reader.Skip();");
                    sb.AppendLine(
                        "                    break;");
                }
            }

            sb.AppendLine("                default:");
            sb.AppendLine("                    reader.Skip();");
            sb.AppendLine("                    break;");

            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        return result;");
            sb.AppendLine("    }");
        }
        private static void GenerateNestedReadHelpers(StringBuilder sb, ITypeSymbol type, HashSet<INamedTypeSymbol> generated)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                GenerateNestedReadHelpers(sb, arrayType.ElementType,  generated);

                return;
            }

            if (type is not INamedTypeSymbol namedType)
                return;

            if (IsList(namedType))
            {
                GenerateNestedReadHelpers(
                    sb,
                    namedType.TypeArguments[0],
                    generated);

                return;
            }

            if (namedType.TypeKind != TypeKind.Class &&
                !HasAttribute(namedType, "DevoidDataClass"))
            {
                return;
            }

            if (!HasAttribute(namedType, "DevoidDataClass"))
                return;

            if (!generated.Add(namedType))
                return;

            GenerateDataClassReadMethod(sb, namedType);

            foreach (ISymbol member in GetSerializableMembers(namedType))
            {
                ITypeSymbol? memberType = member switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    _ => null
                };

                if (memberType is not null)
                    GenerateNestedReadHelpers(
                        sb,
                        memberType,
                        generated);
            }
        }
    }
}
