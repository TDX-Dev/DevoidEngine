using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DevoidEngine.SourceGen.ComponentSerialization
{
    internal static class ComponentCollector
    {
        public static INamedTypeSymbol? GetComponent(
            GeneratorSyntaxContext context,
            CancellationToken _)
        {
            var classNode = (ClassDeclarationSyntax)context.Node;

            var symbol = context.SemanticModel.GetDeclaredSymbol(classNode) as INamedTypeSymbol;

            if (symbol == null)
                return null;

            if (symbol.IsAbstract)
                return null;

            var baseType = symbol.BaseType;

            while (baseType != null)
            {
                if (baseType.Name == "Component")
                    return symbol;

                baseType = baseType.BaseType;
            }

            return null;
        }
    }
}
