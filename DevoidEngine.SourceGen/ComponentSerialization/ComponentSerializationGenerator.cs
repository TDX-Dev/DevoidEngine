using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace DevoidEngine.SourceGen.ComponentSerialization;

[Generator]
public class ComponentSerializationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached)
        //    Debugger.Launch();

        var components = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: ComponentCollector.GetComponent)
            .Where(symbol => symbol != null);

        context.RegisterSourceOutput(
            components,
            (spc, component) =>
            {
                SerializerEmitter.Emit(spc, component!);
            });

        context.RegisterSourceOutput(
            components.Collect(),
            (spc, componentsList) =>
            {
                RegistryEmitter.Emit(spc, componentsList);
            });
    }
}