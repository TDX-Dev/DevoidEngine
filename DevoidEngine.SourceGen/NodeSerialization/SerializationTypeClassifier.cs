using Microsoft.CodeAnalysis;

namespace DevoidEngine.SourceGen.NodeSerialization;

internal static class SerializationTypeClassifier
{
    public static SerializationType Classify(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Int32 => SerializationType.Int32,
            SpecialType.System_Int64 => SerializationType.Int64,
            SpecialType.System_Single => SerializationType.Float,
            SpecialType.System_Double => SerializationType.Double,
            SpecialType.System_Boolean => SerializationType.Bool,
            SpecialType.System_String => SerializationType.String,
            _ => ClassifyComplexType(type)
        };
    }

    private static SerializationType ClassifyComplexType(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Enum)
            return SerializationType.Enum;

        if (type.TypeKind == TypeKind.Array)
            return SerializationType.Array;

        if (type.TypeKind == TypeKind.Struct)
            return SerializationType.Struct;

        if (type.TypeKind == TypeKind.Class)
            return SerializationType.Class;

        return SerializationType.Unsupported;
    }
}