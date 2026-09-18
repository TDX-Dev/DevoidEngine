using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevoidEngine.SourceGen.NodeSerialization
{
    [Generator]
    public sealed class SerializationGenerator : IIncrementalGenerator
    {
        private static bool IsCandidate(INamedTypeSymbol type, INamedTypeSymbol nodeType)
        {
            if (type.DeclaredAccessibility != Accessibility.Public || type.IsStatic || type.IsGenericType)
                return false;

            for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current, nodeType))
                    return true;
            }

            return false;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<INamedTypeSymbol> types = context.SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) => node is TypeDeclarationSyntax,
                    static (ctx, _) => GetTypeSymbol(ctx)).Where(static type => type is not null)!;

            IncrementalValueProvider<INamedTypeSymbol?> nodeType = context.CompilationProvider.Select(static (compilation, _) => compilation.GetTypeByMetadataName("DevoidEngine.Nodes.Node"));

            context.RegisterSourceOutput(
                types.Combine(nodeType),
                static (spc, pair) =>
                {
                    if (pair.Right is not null && IsCandidate(pair.Left, pair.Right))
                        Generate(spc, pair.Left);
                });
        }

        private static INamedTypeSymbol? GetTypeSymbol(GeneratorSyntaxContext context)
        {
            if (context.Node is not TypeDeclarationSyntax syntax)
                return null;

            return (INamedTypeSymbol?)(context.SemanticModel.GetDeclaredSymbol(syntax));
        }

        private static void Generate(SourceProductionContext context, INamedTypeSymbol type)
        {
            NodeSerializerJsonGenerator.Generate(context, type);
        }
    }
}