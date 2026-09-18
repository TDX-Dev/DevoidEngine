using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DevoidEngine.SourceGen.MetadataGeneration
{
    [Generator]
    internal sealed class MetadataGenerator : IIncrementalGenerator
    {
        private static readonly string[] AllowedBaseClasses =
        [
            "DevoidEngine.Nodes.Node"
        ];


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var input = context.CompilationProvider;

            context.RegisterSourceOutput(
                input,
                static (spc, compilation) =>
                {
                    var allowedBaseClasses = GetAllowedBaseClasses(compilation);

                    var classes = CollectClasses(
                        compilation,
                        allowedBaseClasses);

                    foreach (INamedTypeSymbol symbol in classes)
                    {
                        GenerateClass(spc, symbol);
                    }

                    ClassDBRegistryEmitter.Emit(spc, classes);
                });
        }

        private static INamedTypeSymbol[] CollectClasses(
            Compilation compilation,
            INamedTypeSymbol[] allowedBaseClasses)
        {
            var classes = new HashSet<INamedTypeSymbol>(
                SymbolEqualityComparer.Default);

            foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

                var typeDeclarations = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();

                foreach (ClassDeclarationSyntax declaration in typeDeclarations)
                {
                    if (semanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol)
                        continue;

                    if (symbol.IsGenericType)
                        continue;

                    if (IsDerivedFromAllowedBase(symbol, allowedBaseClasses))
                    {
                        classes.Add(symbol);
                    }
                }
            }

            return
            [
                .. classes.OrderBy(static symbol =>
            symbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat))
            ];
        }

        private static bool IsDerivedFromAllowedBase(INamedTypeSymbol symbol, INamedTypeSymbol[] allowedBaseClasses)
        {
            INamedTypeSymbol? current = symbol;

            while (current is not null)
            {
                foreach (INamedTypeSymbol allowedBase in allowedBaseClasses)
                {
                    if (SymbolEqualityComparer.Default.Equals(current, allowedBase))
                        return true;
                }

                current = current.BaseType;
            }

            return false;
        }
        private static void CollectClass(Compilation compilation, INamedTypeSymbol symbol, HashSet<INamedTypeSymbol> classes)
        {
            if (symbol.SpecialType == SpecialType.System_Object)
                return;

            if (symbol.IsGenericType)
                return;

            // Already processed.
            if (!classes.Add(symbol))
                return;

            // Only generate classes belonging to this compilation.
            if (!SymbolEqualityComparer.Default.Equals(
                    symbol.ContainingAssembly,
                    compilation.Assembly))
            {
                classes.Remove(symbol);
                return;
            }

            INamedTypeSymbol? baseType = symbol.BaseType;

            if (baseType is null)
                return;

            CollectClass(
                compilation,
                baseType,
                classes);
        }

        private static void GenerateClass(SourceProductionContext context, INamedTypeSymbol symbol)
        {
            if (symbol.IsGenericType)
                return;

            string className = symbol.Name;
            string classAssemblyQualifiedName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string classType = GetTypeOfName(symbol);
            string baseClass = GenerateBaseClass(symbol);

            var properties = symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(property =>
                    property.DeclaredAccessibility == Accessibility.Public &&
                    IsSupportedType(property.Type))
                .ToArray();

            var fields = symbol
                .GetMembers()
                .OfType<IFieldSymbol>()
                .Where(field =>
                    field.DeclaredAccessibility == Accessibility.Public &&
                    !field.IsConst &&
                    !field.IsStatic &&
                    IsSupportedType(field.Type))
                .ToArray();

            string propertyInfos = string.Join(
                ",\n",
                properties.Select((property, index) =>
                    GeneratePropertyInfo(property, index))
            );

            string fieldInfos = string.Join(
                ",\n",
                fields.Select((field, index) =>
                    GenerateFieldInfo(field, index))
            );

            string accessors = string.Join(
                "\n\n",
                properties.Select((property, index) =>
                    GenerateAccessors(symbol, property, index))
            );

            string fieldAccessors = string.Join(
                "\n\n",
                fields.Select((field, index) =>
                    GenerateFieldAccessors(symbol, field, index))
            );

            string source = $$"""
                        #nullable enable
                using DevoidEngine.Metadata;

                namespace DevoidEngine.Generated;

                internal static class {{className}}_ClassDB
                {
                    public static readonly ClassInfo Info = new()
                    {
                        Name = "{{className}}",
                        AssemblyQualifiedName = "{{classAssemblyQualifiedName}}",
                        ClassType = typeof({{classType}}),
                        BaseClass = {{baseClass}},

                        Properties =
                        [
                            {{propertyInfos}}
                        ],
                        Fields =
                        [
                            {{fieldInfos}}
                        ]
                    };

                    {{accessors}}
                    {{fieldAccessors}}
                }
                """;

            context.AddSource(
                $"{className}.ClassDB.g.cs",
                source
            );
        }

        private static string GeneratePropertyInfo(IPropertySymbol property, int index)
        {
            string propertyType = GetTypeOfName(property.Type);

            string getter = property.GetMethod is not null &&
                            property.GetMethod.DeclaredAccessibility == Accessibility.Public
                ? $"Get_{property.Name}_{index}"
                : "null";

            string setter = property.SetMethod is not null &&
                            property.SetMethod.DeclaredAccessibility == Accessibility.Public
                ? $"Set_{property.Name}_{index}"
                : "null";
            return $$"""
                new PropertyInfo
                {
                    Name = "{{property.Name}}",
                    PropertyType = typeof({{propertyType}}),
                    Getter = {{getter}},
                    Setter = {{setter}}
                }
                """;
        }
        private static string GenerateFieldInfo(IFieldSymbol field, int index)
        {
            string fieldType = GetTypeOfName(field.Type);

            string getter = $"GetField_{field.Name}_{index}";

            string setter = !field.IsReadOnly
                ? $"SetField_{field.Name}_{index}"
                : "null";

            return $$"""
                new FieldInfo
                {
                    Name = "{{field.Name}}",
                    FieldType = typeof({{fieldType}}),
                    Getter = {{getter}},
                    Setter = {{setter}}
                }
                """;
        }

        private static string GenerateFieldAccessors(INamedTypeSymbol classSymbol, IFieldSymbol field, int index)
        {
            string className = classSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat);

            string fieldType = GetTypeOfName(field.Type);

            string getterName = $"GetField_{field.Name}_{index}";
            string setterName = $"SetField_{field.Name}_{index}";

            var output = new System.Text.StringBuilder();

            output.AppendLine($$"""
                private static object? {{getterName}}(object instance)
                {
                    {{className}} self = ({{className}})instance;

                    return self.{{field.Name}};
                }
                """);

            if (!field.IsReadOnly)
            {
                output.AppendLine();

                output.AppendLine($$"""
                    private static void {{setterName}}(
                        object instance,
                        object? value)
                    {
                        {{className}} self = ({{className}})instance;

                        self.{{field.Name}} = ({{fieldType}})value!;
                    }
                    """);
            }

            return output.ToString();
        }
        private static string GenerateAccessors(INamedTypeSymbol classSymbol, IPropertySymbol property, int index)
        {
            string className = classSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat);

            string propertyType = GetTypeOfName(property.Type);

            string getterName = $"Get_{property.Name}_{index}";
            string setterName = $"Set_{property.Name}_{index}";

            var output = new System.Text.StringBuilder();

            if (property.GetMethod is not null && property.GetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                output.AppendLine($$"""
                    private static object? {{getterName}}(object instance)
                    {
                        {{className}} self = ({{className}})instance;

                        return self.{{property.Name}};
                    }
                    """);
            }

            if (property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                if (output.Length > 0)
                    output.AppendLine();

                output.AppendLine($$"""
                    private static void {{setterName}}(
                        object instance,
                        object? value)
                    {
                        {{className}} self = ({{className}})instance;

                        self.{{property.Name}} = ({{propertyType}})value!;
                    }
                    """);
            }

            return output.ToString();
        }

        private static string GetTypeOfName(ITypeSymbol type)
        {
            return type.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static bool IsSupportedType(ITypeSymbol type)
        {
            return !type.IsRefLikeType;
        }

        private static string GenerateBaseClass(INamedTypeSymbol symbol)
        {
            INamedTypeSymbol? baseType = symbol.BaseType;

            if (baseType is null)
                return "null";

            if (baseType.SpecialType == SpecialType.System_Object)
                return "null";

            string baseClassName = baseType.Name;

            return $"{baseClassName}_ClassDB.Info";
        }

        private static INamedTypeSymbol[] GetAllowedBaseClasses(Compilation compilation)
        {
            var result = new List<INamedTypeSymbol>();

            foreach (string name in AllowedBaseClasses)
            {
                INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(name);

                if (symbol is not null)
                    result.Add(symbol);
            }

            return [.. result];
        }
    }
}