using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DevoidEngine.SourceGen;

[Generator]
public class ScriptRegistryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations =
            context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) =>
                {
                    var classSyntax = (ClassDeclarationSyntax)ctx.Node;
                    return ctx.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
                })
            .Where(static symbol => symbol != null);

        var compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndClasses,
            static (spc, source) =>
            {
                var compilation = source.Left;
                var classes = source.Right;

                var componentSymbol =
                    compilation.GetTypeByMetadataName(
                        "DevoidEngine.Engine.Components.Component");

                var scriptRegistrySymbol =
                    compilation.GetTypeByMetadataName(
                        "DevoidRuntime.ScriptRegistry");

                if (componentSymbol == null || scriptRegistrySymbol == null)
                    return;

                var scripts = classes
                    .OfType<INamedTypeSymbol>()
                    .Where(symbol =>
                        !symbol.IsAbstract &&
                        InheritsFrom(symbol, componentSymbol) &&
                        SymbolEqualityComparer.Default.Equals(
                            symbol.ContainingAssembly,
                            compilation.Assembly))
                    .Distinct(SymbolEqualityComparer.Default).
                        Cast<INamedTypeSymbol>().
                        ToList();

                GenerateRegistry(spc, scripts);
            });
    }

    static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        var current = type.BaseType;

        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;

            current = current.BaseType;
        }

        return false;
    }

    static void GenerateRegistry(
        SourceProductionContext context,
        System.Collections.Generic.List<INamedTypeSymbol> scripts)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace DevoidRuntime.Generated;");
        sb.AppendLine();
        sb.AppendLine("using DevoidRuntime;");
        sb.AppendLine("using DevoidEngine.Engine.Components;");
        sb.AppendLine();
        sb.AppendLine("public static class GameScriptRegistry");
        sb.AppendLine("{");
        sb.AppendLine("    public static void RegisterAll()");
        sb.AppendLine("    {");

        foreach (var script in scripts)
        {
            string name = script.ToDisplayString();

            sb.AppendLine($"        ScriptRegistry.Register<{name}>();");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource(
            "GameScriptRegistry.g.cs",
            sb.ToString());
    }
}