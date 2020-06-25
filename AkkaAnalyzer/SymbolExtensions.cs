using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AkkaAnalyzer
{
    public static class SymbolExtensions
    {
        public static ISymbol FindSymbol(this Compilation compilation, Func<ISymbol, bool> predicate) => compilation.SyntaxTrees.SelectMany(x => x.GetRoot()
                                                                                                                                                       .GetAllSymbols(compilation)
                                                                                                                                                       .Where(predicate))
                .FirstOrDefault();
    }
}
