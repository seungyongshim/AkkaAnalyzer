﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Generator
{
    public interface ICodeGenerator
    {
        void Initialize(List<INamedTypeSymbol> allSymbols);

        void WriteClass(INamedTypeSymbol type);

        void WriteInterface(INamedTypeSymbol iface);

        void WriteEnum(INamedTypeSymbol enm);
        void WriteDelegate(INamedTypeSymbol type);
        void Complete();
    }
    interface ICodeDiffGenerator
    {
        void Initialize(List<INamedTypeSymbol> allSymbols, List<INamedTypeSymbol> oldSymbols);

        void WriteClass(INamedTypeSymbol type, INamedTypeSymbol oldType);

        void WriteInterface(INamedTypeSymbol iface, INamedTypeSymbol oldIface);

        void WriteEnum(INamedTypeSymbol enm, INamedTypeSymbol oldEnm);

        void WriteDelegate(INamedTypeSymbol del, INamedTypeSymbol oldDel);

        void Complete();
    }
}
