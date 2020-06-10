using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaAnalyzer
{
    public static class SymbolExtensions
    {
        public static ISymbol FindSymbol(this Compilation compilation, Func<ISymbol, bool> predicate)
        {
            
            return compilation.SyntaxTrees.SelectMany(x => x.GetRoot()
                                                .GetAllSymbols(compilation)
                                                .Where(predicate))
                .FirstOrDefault();
        }
    }
}
