using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace AkkaAnalyzer
{
    public static class SyntaxNodeHelper
    {
        public static ISymbol GetSymbols(this SyntaxNode root, Compilation compilation)
        {
            var model = compilation.GetSemanticModel(root.SyntaxTree);
            ISymbol symbol = model.GetSymbolInfo(root).Symbol;
            return symbol;
        }

        public static IEnumerable<ISymbol> GetAllSymbols(this SyntaxNode root, Compilation compilation)
        {
            var noDuplicates = new HashSet<ISymbol>();

            var model = compilation.GetSemanticModel(root.SyntaxTree);

            foreach (var node in root.DescendantNodesAndSelf())
            {
                switch (node.Kind())
                {

                    case SyntaxKind.ExpressionStatement:
                    case SyntaxKind.InvocationExpression:
                        break;
                    default:
                        ISymbol symbol = model.GetSymbolInfo(node).Symbol;

                        if (symbol != null)
                        {
                            if (noDuplicates.Add(symbol))
                                yield return symbol;
                        }
                        break;
                }
            }
        }

        public static IEnumerable<SyntaxToken> HasRecieveCallee(this BaseMethodDeclarationSyntax method)
        {
            return method.DescendantTokens()
                         .Where(x => x.IsKind(SyntaxKind.IdentifierToken))
                         .Where(x => "Receive".Equals(x.ValueText));

        }

        public static bool IsReceiveActor(this ClassDeclarationSyntax @class)
        {
            return @class.BaseList?.Types
                                    .OfType<SimpleBaseTypeSyntax>()
                                    .Select(x => x.Type as IdentifierNameSyntax)
                                    .Where(x => x != null)
                                    .Where(x => "ReceiveActor".Equals(x.Identifier.ValueText))
                                    .Count() > 0;
        }

        public static string Fullname(this ClassDeclarationSyntax @class)
        {
            if (SyntaxNodeHelper.TryGetParentSyntax(@class, out NamespaceDeclarationSyntax namespaceDeclaration))
            {
                return $"{namespaceDeclaration.Name}.{@class.Identifier.ValueText}";
            }
            return null;
        }

        public static bool TryGetParentSyntax<T>(SyntaxNode syntaxNode, out T result)
            where T : SyntaxNode
        {
            // set defaults
            result = null;

            if (syntaxNode == null)
            {
                return false;
            }

            try
            {
                syntaxNode = syntaxNode.Parent;

                if (syntaxNode == null)
                {
                    return false;
                }

                if (syntaxNode.GetType() == typeof(T))
                {
                    result = syntaxNode as T;
                    return true;
                }

                return TryGetParentSyntax<T>(syntaxNode, out result);
            }
            catch
            {
                return false;
            }
        }
    }
}
